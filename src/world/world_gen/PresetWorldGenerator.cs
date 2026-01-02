using Godot;

[GlobalClass]
public partial class PresetWorldGenerator : WorldGenerator
{
	[Export] public Godot.Collections.Array<Vector4I> Blocks { get; set; }
	[Export] public Vector3I Offset { get; set; }

	public PresetWorldGenerator()
	{
	}

	public override Module GenerateModules(Vector3I moduleLocation, Material moduleMaterial, int moduleSize)
	{
		var module = new Module(moduleSize, moduleMaterial);

		foreach (var block in Blocks)
		{
			var worldPos = new Vector3I(block.X, block.Y, block.Z) + Offset;
			var inModulePos = Module.WorldToInModulePos(worldPos, moduleSize, moduleLocation);

			if (module.IsInModule(inModulePos))
			{
				module.SetBlock(inModulePos, block.W);
			}
		}

		return module;
	}

	public override void SetSeed(int seed)
	{
	}
}
