using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class BiomeWorldGenerator : ConstructGenerator
{
	[Export] Godot.Collections.Array<Biome> Biomes;

	private FastNoiseLite _noise = new FastNoiseLite();

	public BiomeWorldGenerator()
	{
		_noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
	}

	public override void SetSeed(int seed)
	{
		for (int i = 0; i < Biomes.Count; i++)
		{
			Biomes[i].SetSeed(seed);
		}
	}

	public override GenerationResponse GenerateModules(
		Vector3I moduleLocation,
		Material moduleMaterial,
		HashSet<Vector3I> prevLoaded
	)
	{
		Module module = new Module(moduleSize, moduleMaterial);
		PopulateModule(module, moduleLocation, moduleSize);
		return new GenerationResponse
		{
			generatedAllModules = false,
			generatedModules = new Dictionary<Vector3I, Module>
			{
				{moduleLocation, module}
			}
		};
	}

	private void PopulateModule(Module module, Vector3I moduleLocation, int moduleSize)
	{
		Vector3I rootWorldPos = moduleLocation * moduleSize;
		for (int x = 0; x < moduleSize; x++)
		{
			for (int z = 0; z < moduleSize; z++)
			{
				Biome biome = Biomes[0];
				Vector2I worldLocation = new Vector2I(rootWorldPos.X, rootWorldPos.Z) + new Vector2I(x, z);
				int groundHeight = biome.GetGroundHeight(worldLocation);
				int maxY = Mathf.Min(groundHeight - moduleLocation.Y * moduleSize, moduleSize);

				for (int y = 0; y < maxY; y++)
				{
					Vector3I inModulePos = new Vector3I(x, y, z);
					Vector3I worldPos = inModulePos + rootWorldPos;
					int blockId = biome.GetBlockId(worldPos, groundHeight);
					module.SetBlock(inModulePos, blockId);
				}
			}
		}
	}

	public override bool IsModuleNeeded(Vector3I moduleLocation)
	{
		Vector3I rootWorldPos = moduleLocation * moduleSize;
		for (int x = 0; x < moduleSize; x++)
		{
			for (int z = 0; z < moduleSize; z++)
			{
				Biome biome = Biomes[0];
				Vector2I worldLocation = new Vector2I(rootWorldPos.X, rootWorldPos.Z) + new Vector2I(x, z);
				int groundHeight = biome.GetGroundHeight(worldLocation);
				int maxY = Mathf.Min(groundHeight - moduleLocation.Y * moduleSize, moduleSize);
				if ((maxY / moduleSize) >= moduleLocation.Y)
				{
					return true;
				}
			}
		}

		return false;
	}

}
