using System;
using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class PresetConstructGenerator : ConstructGenerator
{
	[Export] public Godot.Collections.Array<Vector4I> Blocks { get; set; }
	[Export] public Vector3I Offset { get; set; }
	private HashSet<ModuleLocation> requiredModules = [];

	public override void Init(int moduleSize, int seed)
	{
		base.Init(moduleSize, seed);
		requiredModules = [];
		foreach (Vector4I block in Blocks)
		{
			ConstructGridPos constructPos = new(new Vector3I(block.X, block.Y, block.Z));
			requiredModules.Add(constructPos.ToModuleLocation(moduleSize));
		}
	}

	public override GenerationResponse GenerateModules(
		ModuleLocation moduleLocation,
		Material moduleMaterial,
		HashSet<ModuleLocation> prevLoaded = null
	)
	{
		Module module = new Module(ModuleSize, moduleMaterial);

		ModuleGridPos minPos = new(Vector3I.One * ModuleSize);
		ModuleGridPos maxPos = new(Vector3I.Zero);
		foreach (Vector4I block in Blocks)
		{
			ConstructGridPos inConstructBlockPos = new ConstructGridPos(new Vector3I(block.X, block.Y, block.Z) + Offset);
			ModuleGridPos inModuleBlockPos = inConstructBlockPos.ToModule(ModuleSize);

			minPos = new(minPos.Value.Min(inConstructBlockPos.Value));
			maxPos = new(maxPos.Value.Max(inConstructBlockPos.Value));

			if (module.IsInModule(inConstructBlockPos, moduleLocation))
			{
				module.SetBlock(inModuleBlockPos, block.W);
			}
		}

		return new GenerationResponse
		{
			generatedAllModules = false,
			generatedModules = new Dictionary<ModuleLocation, Module>
			{
				{moduleLocation, module}
			},
			maxBlockPos = maxPos,
			minBlockPos = minPos
		};
	}

	public override bool IsModuleNeeded(ModuleLocation moduleLocation)
	{
		return requiredModules.Contains(moduleLocation);
	}
}
