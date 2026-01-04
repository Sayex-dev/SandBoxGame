using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Construct : Node
{
	private Dictionary<Vector3I, Module> loadedModules = new();
	private List<Vector3I> queuedModulesPositions = new();

	private int moduleSize;
	private ConstructGenerator constructGenerator;

	public Construct(int moduleSize, ConstructGenerator constructGenerator)
	{
		this.moduleSize = moduleSize;
		this.constructGenerator = constructGenerator;
	}

	public void SetBlockState(Vector3I inConstructPos, BlockState blockState)
	{
		var moduleLoc = Module.WorldToModuleLocation(inConstructPos, moduleSize);
		var modulePos = Module.WrapToModule(inConstructPos, moduleSize);
		loadedModules[moduleLoc].SetBlockState(modulePos, blockState);
	}

	public BlockState GetBlockState(Vector3I inConstructPos)
	{
		var moduleLoc = Module.WorldToModuleLocation(inConstructPos, moduleSize);
		Module module = loadedModules[moduleLoc];
		var modulePos = Module.WorldToInModulePos(inConstructPos, module.ModuleSize, moduleLoc);
		return module.GetBlockState(modulePos);
	}

	public bool HasBlockState(Vector3I inConstructPos)
	{
		var moduleLoc = Module.WorldToModuleLocation(inConstructPos, moduleSize);
		var modulePos = Module.WrapToModule(inConstructPos, moduleSize);
		return loadedModules[moduleLoc].HasBlockState(modulePos);
	}

	public void LoadPosition(Vector3 inConstructPos, Vector3I renderDistance)
	{
		var loadModulePos = (Vector3I)(inConstructPos / moduleSize).Floor();

		var desiredModules = new List<Vector3I>();
		var addModules = new List<Vector3I>();
		var removeModules = new List<Vector3I>();

		for (int x = -renderDistance.X; x < renderDistance.X; x++)
		{
			for (int y = -renderDistance.Y; y < renderDistance.Y; y++)
			{
				for (int z = -renderDistance.Z; z < renderDistance.Z; z++)
				{
					desiredModules.Add(loadModulePos + new Vector3I(x, y, z));
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
		Dictionary<Vector3I, Task<Module>> moduleJobs = [];
		foreach (var modulePos in loadPositions)
		{
			moduleJobs[modulePos] = Task.Run(() =>
			{
				Module module = constructGenerator.GenerateModules(modulePos, moduleMaterial, moduleSize);
				module.BuildMesh(blockStore);
				module.Position = (Vector3)(moduleSize * modulePos);
				return module;
			});
		}

		foreach (var kvp in moduleJobs)
		{
			Vector3I modulePos = kvp.Key;
			Task<Module> genTask = kvp.Value;
			Module module = await genTask;

			if (queuedModulesPositions.Contains(modulePos))
			{
				queuedModulesPositions.Remove(modulePos);
				loadedModules[modulePos] = module;
				AddChild(module);
			}
			else
			{
				module.QueueFree();
			}
		}
	}
}
