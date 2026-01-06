using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class PresetConstructGenerator : ConstructGenerator
{
	[Export] public Godot.Collections.Array<Vector4I> Blocks { get; set; }
	[Export] public Vector3I Offset { get; set; }
	private HashSet<Vector3I> requiredChunks;

	public override void Init(int moduleSize)
	{
		base.Init(moduleSize);
		requiredChunks = [];
		foreach (Vector4 block in Blocks)
		{
			requiredChunks.Add((Vector3I)(new Vector3(block.X, block.Y, block.Z) / moduleSize));
		}
	}

	public override GenerationResponse GenerateModules(
		Vector3I relativeWorldPos,
		Material moduleMaterial,
		HashSet<Vector3I> prevLoaded = null
	)
	{
		Module module = new Module(moduleSize, moduleMaterial);
		Vector3I moduleLocation = Module.InConstructToModuleLocation(relativeWorldPos, moduleSize);

		foreach (var block in Blocks)
		{
			var worldPos = new Vector3I(block.X, block.Y, block.Z) + Offset;
			var inModulePos = Module.InConstructToInModulePos(worldPos, moduleSize, relativeWorldPos);

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

	public override bool IsModuleNeeded(Vector3I chunkLocation)
	{
		return requiredChunks.Contains(chunkLocation);
	}

	public override void SetSeed(int seed)
	{
	}
}
