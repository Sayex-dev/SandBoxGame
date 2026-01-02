using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class BlockWorld : Node3D
{
	private int seed;
	private int moduleSize;
	private BlockStore blockStore;
	private WorldGenerator worldGen;
	private Material moduleMaterial;
	private AbilityManager abilityManager;
	private Dictionary<Vector3I, Construct> constructs = [];
	public BlockWorld(
		int seed,
		int moduleSize,
		BlockStore blockStore,
		WorldGenerator worldGen,
		Material moduleMaterial,
		AbilityManager abilityManager
	)
	{
		this.seed = seed;
		this.moduleSize = moduleSize;
		this.blockStore = blockStore;
		this.worldGen = worldGen;
		this.moduleMaterial = moduleMaterial;
		this.abilityManager = abilityManager;
	}

	public void SetBlockState(Vector3I worldPos, BlockState blockState)
	{
		var moduleLoc = Module.WorldToModuleLocation(worldPos, moduleSize);
		var modulePos = Module.WrapToModule(worldPos, moduleSize);
		modules[moduleLoc].SetBlockState(modulePos, blockState);
	}

	public BlockState GetBlockState(Vector3I worldPos)
	{
		var moduleLoc = Module.WorldToModuleLocation(worldPos, moduleSize);
		Module module = modules[moduleLoc];
		var modulePos = Module.WorldToInModulePos(worldPos, module.ModuleSize, moduleLoc);
		return module.GetBlockState(modulePos);
	}

	public bool HasBlockState(Vector3I worldPos)
	{
		var moduleLoc = Module.WorldToModuleLocation(worldPos, moduleSize);
		var modulePos = Module.WrapToModule(worldPos, moduleSize);
		return modules[moduleLoc].HasBlockState(modulePos);
	}

	public void LoadPosition(Vector3 worldPos, Vector3I renderDistance)
	{
		var loadModulePos = (Vector3I)(worldPos / moduleSize).Floor();

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
			if (!modules.ContainsKey(modulePos) && !queuedModulesPositions.Contains(modulePos))
			{
				queuedModulesPositions.Add(modulePos);
				addModules.Add(modulePos);
			}
		}

		foreach (var modulePos in new List<Vector3I>(modules.Keys))
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
			if (modules.TryGetValue(modulePos, out var module))
			{
				modules.Remove(modulePos);
				module.QueueFree();
			}
		}

		// Spawn async module generation task
		_ = GenerateModulesAsync(loadPositions);
	}

	private async Task GenerateModulesAsync(List<Vector3I> loadPositions)
	{
		Dictionary<Vector3I, Task<Module>> moduleJobs = [];
		foreach (var modulePos in loadPositions)
		{
			moduleJobs[modulePos] = Task.Run(() =>
			{
				Module module = worldGen.GenerateModules(modulePos, moduleMaterial, moduleSize);
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
				modules[modulePos] = module;
				AddChild(module);
			}
			else
			{
				module.QueueFree();
			}
		}
	}
}