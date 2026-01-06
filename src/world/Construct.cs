using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Construct : Node3D, IHaveBoundingBox
{
	private Dictionary<Vector3I, Module> loadedModules = new();
	private List<Vector3I> queuedModulesPositions = new();

	private int moduleSize;
	private ConstructGenerator constructGenerator;
	public Vector3I WorldOffset { get; private set; }
	private Vector3I minModuleLocation;
	private Vector3I maxModuleLocation;
	private BlockStore blockStore;
	private Material moduleMaterial;
	private const int MaxConcurrentModuleLoads = 5;
	private SecondOrderDynamics sod;

	public Construct(int moduleSize, ConstructGenerator constructGenerator, Vector3I worldOffset, BlockStore blockStore, Material moduleMaterial, SecondOrderDynamicsSettings sodSettings)
	{
		this.moduleSize = moduleSize;
		this.constructGenerator = constructGenerator;
		WorldOffset = worldOffset;
		this.blockStore = blockStore;
		this.moduleMaterial = moduleMaterial;

		sod = sodSettings.GetInstance(worldOffset);

		Position = worldOffset;
		minModuleLocation = Vector3I.Zero;
		maxModuleLocation = Vector3I.Zero;
	}

	public override void _PhysicsProcess(double delta)
	{
		Position = sod.Update((float)delta, WorldOffset);
	}

	public void MoveTo(Vector3I worldPos)
	{
		WorldOffset = worldPos;
	}

	public void SetBlockState(Vector3I worldPos, BlockState blockState)
	{
		Vector3I inConstructPos = worldPos - WorldOffset;
		var moduleLoc = Module.InConstructToModuleLocation(inConstructPos, moduleSize);
		var modulePos = Module.WrapToModule(inConstructPos, moduleSize);
		loadedModules[moduleLoc].SetBlockState(modulePos, blockState);
	}

	public BlockState GetBlockState(Vector3I worldPos)
	{
		Vector3I inConstructPos = worldPos - WorldOffset;
		var moduleLoc = Module.InConstructToModuleLocation(inConstructPos, moduleSize);
		Module module = loadedModules[moduleLoc];
		var modulePos = Module.InConstructToInModulePos(inConstructPos, module.ModuleSize, moduleLoc);
		return module.GetBlockState(modulePos);
	}

	public bool HasBlockState(Vector3I worldPos)
	{
		Vector3I inConstructPos = worldPos - WorldOffset;
		var moduleLoc = Module.InConstructToModuleLocation(inConstructPos, moduleSize);
		var modulePos = Module.WrapToModule(inConstructPos, moduleSize);
		return loadedModules[moduleLoc].HasBlockState(modulePos);
	}

	public void SetBlock(Vector3I worldPos, int blockId)
	{
		Vector3I inConstructPos = worldPos - WorldOffset;
		Vector3I moduleLocation = Module.InConstructToModuleLocation(inConstructPos, moduleSize);
		Vector3I inModulePosition = Module.InConstructToInModulePos(inConstructPos, moduleSize, moduleLocation);
		loadedModules[moduleLocation].SetBlock(inModulePosition, blockId);
	}

	public int GetBlock(Vector3I worldPos)
	{
		Vector3I inConstructPos = worldPos - WorldOffset;
		Vector3I moduleLocation = Module.InConstructToModuleLocation(inConstructPos, moduleSize);
		Vector3I inModulePosition = Module.InConstructToInModulePos(inConstructPos, moduleSize, moduleLocation);
		return loadedModules[moduleLocation].GetBlock(inModulePosition);
	}

	public void LoadPosition(Vector3 worldPos, Vector3I renderDistance)
	{
		var loadModulePos = (Vector3I)((worldPos - WorldOffset) / moduleSize).Floor();

		var desiredModules = new List<Vector3I>();
		var addModules = new List<Vector3I>();
		var removeModules = new List<Vector3I>();

		for (int x = -renderDistance.X; x < renderDistance.X; x++)
		{
			for (int y = -renderDistance.Y; y < renderDistance.Y; y++)
			{
				for (int z = -renderDistance.Z; z < renderDistance.Z; z++)
				{
					Vector3I pos = loadModulePos + new Vector3I(x, y, z);
					if (constructGenerator.IsModuleNeeded(pos))
					{
						desiredModules.Add(pos);
					}
				}
			}
		}

		foreach (var modulePos in desiredModules)
		{
			if (!loadedModules.ContainsKey(modulePos) && !queuedModulesPositions.Contains(modulePos))
			{
				queuedModulesPositions.Add(modulePos);
				addModules.Add(modulePos);
			}
		}

		foreach (var modulePos in new List<Vector3I>(loadedModules.Keys))
		{
			if (!desiredModules.Contains(modulePos))
				removeModules.Add(modulePos);

			if (queuedModulesPositions.Contains(modulePos))
				queuedModulesPositions.Remove(modulePos);
		}

		UpdateModuleLoading(addModules, removeModules);
	}

	public void UpdateModuleLoading(
	List<Vector3I> loadPositions = null,
	List<Vector3I> unloadPositions = null
)
	{
		loadPositions ??= new List<Vector3I>();
		unloadPositions ??= new List<Vector3I>();

		// Unload immediately on main thread
		foreach (var modulePos in unloadPositions)
		{
			if (loadedModules.TryGetValue(modulePos, out var module))
			{
				loadedModules.Remove(modulePos);
				module.QueueFree();
			}
		}

		// Spawn async module generation task
		GenerateModulesAsync(loadPositions);
	}

	private async void GenerateModulesAsync(List<Vector3I> loadPositions)
	{
		List<Task<GenerationResponse>> moduleJobs = [];
		int i = 0;
		foreach (var modulePos in loadPositions)
		{
			i++;
			if (i % MaxConcurrentModuleLoads == 0)
			{
				await ToSignal(GetTree(), "process_frame");
			}

			moduleJobs.Add(Task.Run(() =>
			{
				GenerationResponse response = constructGenerator.GenerateModules(modulePos, moduleMaterial);
				Dictionary<Vector3I, Module> modules = response.generatedModules;
				foreach (KeyValuePair<Vector3I, Module> entry in modules)
				{
					entry.Value.BuildMesh(blockStore);
					entry.Value.Position = (Vector3)(moduleSize * entry.Key);
				}
				return response;
			}));
		}

		i = 0;
		foreach (Task<GenerationResponse> job in moduleJobs)
		{
			i++;
			if (i % MaxConcurrentModuleLoads == 0)
			{
				await ToSignal(GetTree(), "process_frame");
			}

			GenerationResponse response = await job;
			foreach (KeyValuePair<Vector3I, Module> entry in response.generatedModules)
			{
				Vector3I modulePos = entry.Key;
				Module module = entry.Value;

				if (queuedModulesPositions.Contains(modulePos))
				{
					queuedModulesPositions.Remove(modulePos);
					loadedModules[modulePos] = module;
					AddChild(module);

					minModuleLocation = new Vector3I
					{
						X = Math.Min(minModuleLocation.X, modulePos.X),
						Y = Math.Min(minModuleLocation.Y, modulePos.Y),
						Z = Math.Min(minModuleLocation.Z, modulePos.Z),
					};
					maxModuleLocation = new Vector3I
					{
						X = Math.Max(maxModuleLocation.X, modulePos.X),
						Y = Math.Max(maxModuleLocation.Y, modulePos.Y),
						Z = Math.Max(maxModuleLocation.Z, modulePos.Z),
					};
				}
				else
				{
					module.QueueFree();
				}
			}
		}
	}

	public Vector3I GetRootPos()
	{
		return WorldOffset;
	}

	public Vector3I GetMin()
	{
		return WorldOffset + minModuleLocation * moduleSize;
	}

	public Vector3I GetMax()
	{
		return WorldOffset + maxModuleLocation * moduleSize;
	}
}
