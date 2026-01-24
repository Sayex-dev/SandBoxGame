using System.Collections.Generic;
using Godot;


public class PresetConstructGenerator : ConstructGenerator
{

	private List<Vector4I> blocks;
	private Vector3I offset;
	private HashSet<ModuleLocation> requiredModules = [];

	public PresetConstructGenerator(
		int moduleSize,
		int seed,
		List<Vector4I> blocks,
		Vector3I offset
	) : base(moduleSize, seed)
	{
		this.blocks = blocks;
		this.offset = offset;

		requiredModules = [];
		foreach (Vector4I block in blocks)
		{
			ConstructGridPos constructPos = new(new Vector3I(block.X, block.Y, block.Z));
			requiredModules.Add(constructPos.ToModuleLocation(moduleSize));
		}
	}

	public override ModuleGenerationResponse GenerateModules(
		ModuleLocation moduleLocation,
		HashSet<ModuleLocation> prevLoaded = null
	)
	{
		Module module = new Module(moduleSize);

		ModuleGridPos minPos = new(Vector3I.One * moduleSize);
		ModuleGridPos maxPos = new(Vector3I.Zero);
		foreach (Vector4I block in blocks)
		{
			ConstructGridPos inConstructBlockPos = new ConstructGridPos(new Vector3I(block.X, block.Y, block.Z) + offset);
			ModuleGridPos inModuleBlockPos = inConstructBlockPos.ToModule(moduleSize);

			minPos = new(minPos.Value.Min(inConstructBlockPos.Value));
			maxPos = new(maxPos.Value.Max(inConstructBlockPos.Value));

			if (module.IsInModule(inConstructBlockPos, moduleLocation))
			{
				module.SetBlock(inModuleBlockPos, block.W);
			}
		}

		return new ModuleGenerationResponse
		{
			GeneratedAllModules = false,
			GeneratedModules = new Dictionary<ModuleLocation, Module>
			{
				{moduleLocation, module}
			},
			MaxBlockPos = maxPos,
			MinBlockPos = minPos
		};
	}

	public override bool IsModuleNeeded(ModuleLocation moduleLocation)
	{
		return requiredModules.Contains(moduleLocation);
	}
}
