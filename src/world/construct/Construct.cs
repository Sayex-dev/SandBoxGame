using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


[GlobalClass]
public partial class Construct : Node3D, IHaveBoundingBox
{
	[Export] private ConstructGenerator constructGenerator;
	[Export] private SecondOrderDynamicsSettings sodSettings;
	[Export] public bool IsGlobal = false;
	public int ModuleSize { get; private set; }
	public ConstructTransform ConstructTransform { get; private set; }

	private Dictionary<ModuleLocation, Module> loadedModules = new();
	private List<ModuleLocation> queuedModulesPositions = new();

	private ModuleLocation minModuleLocation;
	private ModuleLocation maxModuleLocation;
	private BlockStore blockStore;
	private Material moduleMaterial;
	private const int MaxConcurrentModuleLoads = 5;
	private SecondOrderDynamics sod;
	private bool isStatic;

	public void SetupConstruct(
		int moduleSize,
		BlockStore blockStore,
		Material moduleMaterial,
		ConstructGenerator constructGenerator = null,
		ConstructTransform constructTransform = null,
		SecondOrderDynamics sod = null
	)
	{
		this.blockStore = blockStore;
		this.moduleMaterial = moduleMaterial;
		ModuleSize = moduleSize;

		if (constructTransform == null)
		{
			ConstructTransform = new ConstructTransform(new((Vector3I)Position), Direction.FORWARD);
		}
		else
		{
			ConstructTransform = constructTransform;
		}
		Position = ConstructTransform.WorldPos.Value;

		if (sodSettings == null)
		{
			this.sod = sod;
		}
		else
		{
			this.sod = sodSettings.GetInstance(ConstructTransform.WorldPos.Value);
		}

		if (this.constructGenerator == null) this.constructGenerator = constructGenerator;

		minModuleLocation = new ModuleLocation(Vector3I.Zero);
		maxModuleLocation = new ModuleLocation(Vector3I.Zero);

		SetPhysicsProcess(true);
	}

	public override void _EnterTree()
	{
		SetPhysicsProcess(false);
	}

	public override void _PhysicsProcess(double delta)
	{
		Position = sod.Update((float)delta, ConstructTransform.WorldPos.Value);
	}

	public void MoveTo(WorldGridPos worldPos)
	{
		if (worldPos == ConstructTransform.WorldPos) return;
		ConstructTransform.MoveTo(worldPos);
	}

	public void RotateTo(Direction dir)
	{
		if (dir == Direction.UP || dir == Direction.DOWN) return;
	}

	public void SetBlockState(WorldGridPos worldPos, BlockState blockState)
	{
		ModuleLocation moduleLocation = worldPos.ToModuleLocation(ConstructTransform, ModuleSize);
		ModuleGridPos inModulePos = worldPos.ToModule(ConstructTransform, ModuleSize);
		Module module = loadedModules[moduleLocation];
		module.SetBlockState(inModulePos, blockState);
	}

	public BlockState GetBlockState(WorldGridPos worldPos)
	{
		ModuleLocation moduleLocation = worldPos.ToModuleLocation(ConstructTransform, ModuleSize);
		ModuleGridPos inModulePos = worldPos.ToModule(ConstructTransform, ModuleSize);
		Module module = loadedModules[moduleLocation];
		return module.GetBlockState(inModulePos);
	}

	public bool HasBlockState(WorldGridPos worldPos)
	{
		ModuleLocation moduleLocation = worldPos.ToModuleLocation(ConstructTransform, ModuleSize);
		ModuleGridPos inModulePos = worldPos.ToModule(ConstructTransform, ModuleSize);
		Module module = loadedModules[moduleLocation];
		return module.HasBlockState(inModulePos);
	}

	public void SetBlock(WorldGridPos worldPos, int blockId)
	{
		ModuleLocation moduleLocation = worldPos.ToModuleLocation(ConstructTransform, ModuleSize);
		ModuleGridPos inModulePos = worldPos.ToModule(ConstructTransform, ModuleSize);
		Module module = loadedModules[moduleLocation];
		module.SetBlock(inModulePos, blockId);
	}

	public int GetBlock(WorldGridPos worldPos)
	{
		ModuleLocation moduleLocation = worldPos.ToModuleLocation(ConstructTransform, ModuleSize);
		ModuleGridPos inModulePos = worldPos.ToModule(ConstructTransform, ModuleSize);
		Module module = loadedModules[moduleLocation];
		return module.GetBlock(inModulePos);
	}

	public bool HasBlock(WorldGridPos worldPos, out int blockId)
	{
		blockId = -1;
		ModuleLocation moduleLocation = worldPos.ToModuleLocation(ConstructTransform, ModuleSize);
		ModuleGridPos inModulePos = worldPos.ToModule(ConstructTransform, ModuleSize);
		Module module;

		if (!loadedModules.TryGetValue(moduleLocation, out module)) return false;
		if (!module.HasBlock(inModulePos)) return false;
		blockId = module.GetBlock(inModulePos);
		return blockId != -1;
	}

	public bool HasBlock(WorldGridPos worldPos)
	{
		return HasBlock(worldPos, out _);
	}

	public Dictionary<ModuleLocation, Module> GetModules()
	{
		return loadedModules;
	}

	public void LoadPosition(WorldGridPos worldPos, Vector3I renderDistance)
	{
		ModuleLocation moduleLocation = worldPos.ToModuleLocation(ConstructTransform, ModuleSize);

		var desiredModules = new List<ModuleLocation>();
		var addModules = new List<ModuleLocation>();
		var removeModules = new List<ModuleLocation>();

		for (int x = -renderDistance.X; x < renderDistance.X; x++)
		{
			for (int y = -renderDistance.Y; y < renderDistance.Y; y++)
			{
				for (int z = -renderDistance.Z; z < renderDistance.Z; z++)
				{
					ModuleLocation newModulePos = new(moduleLocation.Value + new Vector3I(x, y, z));
					if (constructGenerator.IsModuleNeeded(newModulePos))
					{
						desiredModules.Add(newModulePos);
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

		foreach (var modulePos in new List<ModuleLocation>(loadedModules.Keys))
		{
			if (!desiredModules.Contains(modulePos))
				removeModules.Add(modulePos);

			if (queuedModulesPositions.Contains(modulePos))
				queuedModulesPositions.Remove(modulePos);
		}

		UpdateModuleLoading(addModules, removeModules);
	}

	public void UpdateModuleLoading(
	List<ModuleLocation> loadPositions = null,
	List<ModuleLocation> unloadPositions = null
)
	{
		loadPositions ??= new List<ModuleLocation>();
		unloadPositions ??= new List<ModuleLocation>();

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

	private async void GenerateModulesAsync(List<ModuleLocation> loadPositions)
	{
		List<Task<GenerationResponse>> moduleJobs = [];
		int i = 0;
		foreach (ModuleLocation modulePos in loadPositions)
		{
			i++;
			if (i % MaxConcurrentModuleLoads == 0)
			{
				await ToSignal(GetTree(), "process_frame");
			}

			moduleJobs.Add(Task.Run(() =>
			{
				GenerationResponse response = constructGenerator.GenerateModules(modulePos, moduleMaterial);
				Dictionary<ModuleLocation, Module> modules = response.generatedModules;
				foreach (KeyValuePair<ModuleLocation, Module> entry in modules)
				{
					entry.Value.BuildMesh(blockStore);
					entry.Value.Position = (Vector3)(ModuleSize * entry.Key.Value);
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
			foreach (KeyValuePair<ModuleLocation, Module> entry in response.generatedModules)
			{
				ModuleLocation modulePos = entry.Key;
				Module module = entry.Value;

				if (queuedModulesPositions.Contains(modulePos))
				{
					queuedModulesPositions.Remove(modulePos);
					loadedModules[modulePos] = module;
					AddChild(module);

					minModuleLocation = new(new Vector3I
					{
						X = Math.Min(minModuleLocation.Value.X, modulePos.Value.X),
						Y = Math.Min(minModuleLocation.Value.Y, modulePos.Value.Y),
						Z = Math.Min(minModuleLocation.Value.Z, modulePos.Value.Z),
					});
					maxModuleLocation = new(new Vector3I
					{
						X = Math.Max(maxModuleLocation.Value.X, modulePos.Value.X),
						Y = Math.Max(maxModuleLocation.Value.Y, modulePos.Value.Y),
						Z = Math.Max(maxModuleLocation.Value.Z, modulePos.Value.Z),
					});
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
		return ConstructTransform.WorldPos.Value;
	}

	public Vector3I GetMin()
	{
		return minModuleLocation.ToWorld(ModuleSize, ConstructTransform).Value;
	}

	public Vector3I GetMax()
	{
		return maxModuleLocation.ToWorld(ModuleSize, ConstructTransform).Value;
	}

}
