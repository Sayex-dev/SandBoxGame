using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class PresetConstructGenerator : ConstructGenerator
{
	[Export] public Godot.Collections.Array<Vector4I> Blocks { get; set; }
	[Export] public Vector3I Offset { get; set; }

	public override GenerationResponse GenerateModules(Vector3I relativeWorldPos, Material moduleMaterial, int moduleSize)
	{
		Module module = new Module(moduleSize, moduleMaterial);
		Vector3I moduleLocation = Module.WorldToModuleLocation(relativeWorldPos, moduleSize);

		foreach (var block in Blocks)
		{
			var worldPos = new Vector3I(block.X, block.Y, block.Z) + Offset;
			var inModulePos = Module.WorldToInModulePos(worldPos, moduleSize, relativeWorldPos);

			if (module.IsInModule(inModulePos))
			{
				module.SetBlock(inModulePos, block.W);
			}
		}

		return new GenerationResponse
		{
			generatedAllModules = false,
			generatedModules = new Dictionary<Vector3I, Module>
			{
				{moduleLocation, module}
			}
		};
	}

	public override void SetSeed(int seed)
	{
	}
}
