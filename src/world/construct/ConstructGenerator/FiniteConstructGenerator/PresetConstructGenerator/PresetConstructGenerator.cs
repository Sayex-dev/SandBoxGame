using System.Collections.Generic;
using Godot;


public class PresetConstructGenerator : ConstructGenerator
{

	private Dictionary<Vector3I, Block> blocks;
	private Vector3I offset;
	private HashSet<ModuleLocation> requiredModules = [];

	public PresetConstructGenerator(
		int moduleSize,
		int seed,
		Dictionary<Vector3I, Block> blocks,
		Vector3I offset
	) : base(moduleSize, seed)
	{
		this.blocks = blocks;
		this.offset = offset;

		requiredModules = [];
		foreach ((Vector3I pos, Block block) in blocks)
		{
			ConstructGridPos constructPos = new(new Vector3I(pos.X, pos.Y, pos.Z));
			requiredModules.Add(constructPos.ToModuleLocation(moduleSize));
		}
	}

	public override ModuleBlockGenerationResponse GenerateModules(
		ModuleLocation moduleLocation,
		HashSet<ModuleLocation> prevLoaded = null
	)
	{
		Module module = new Module(moduleSize);

		ModuleGridPos minPos = new(Vector3I.One * moduleSize);
		ModuleGridPos maxPos = new(Vector3I.Zero);
		foreach ((Vector3I pos, Block block) in blocks)
		{
			ConstructGridPos inConstructBlockPos = new ConstructGridPos(new Vector3I(pos.X, pos.Y, pos.Z) + offset);
			ModuleGridPos inModuleBlockPos = inConstructBlockPos.ToModule(moduleSize);

			minPos = new(minPos.Value.Min(inConstructBlockPos.Value));
			maxPos = new(maxPos.Value.Max(inConstructBlockPos.Value));

			if (module.IsInModule(inConstructBlockPos, moduleLocation))
			{
				module.SetBlock(inModuleBlockPos, block);
			}
		}

		return new ModuleBlockGenerationResponse
		{
			GeneratedAllModules = false,
			GeneratedModules = new Dictionary<ModuleLocation, Module>
			{
				{moduleLocation, module}
			},
		};
	}

	public override bool IsModuleNeeded(ModuleLocation moduleLocation)
	{
		return requiredModules.Contains(moduleLocation);
	}

	public override HashSet<ModuleLocation> GetAllRequiredModules()
	{
		return requiredModules;
	}
}
