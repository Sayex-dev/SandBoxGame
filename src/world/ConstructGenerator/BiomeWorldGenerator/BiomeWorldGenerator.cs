using System;
using System.Collections.Generic;
using Godot;


public partial class BiomeWorldGenerator : ConstructGenerator
{
	private List<Biome> biomes;
	private FastNoiseLite noise = new();

	private Dictionary<Vector2I, int> cachedMaxModuleY = [];

	public BiomeWorldGenerator(
		int moduleSize,
		int seed,
		List<Biome> biomes
	) : base(moduleSize, seed)
	{
		this.biomes = biomes;

		noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
		noise.Seed = seed;
	}

	public override ModuleBlockGenerationResponse GenerateModules(
		ModuleLocation moduleLocation,
		HashSet<ModuleLocation> prevLoaded
	)
	{
		Module module = new Module(moduleSize);
		PopulateModule(module, moduleLocation, moduleSize);

		return new ModuleBlockGenerationResponse
		{
			GeneratedAllModules = false,
			GeneratedModules = new Dictionary<ModuleLocation, Module>
			{
				{ moduleLocation, module }
			},
		};
	}

	private void PopulateModule(Module module, ModuleLocation moduleLocation, int moduleSize)
	{
		int maxMaxY = 0;
		ConstructGridPos moduleOffset = moduleLocation.ToConstruct(moduleSize);

		for (int x = 0; x < moduleSize; x++)
		{
			for (int z = 0; z < moduleSize; z++)
			{
				Biome biome = biomes[0]; // biome selection later
				Vector2I inConstructLocation =
					new(moduleOffset.Value.X + x, moduleOffset.Value.Z + z);

				int groundHeight = biome.GetGroundHeight(inConstructLocation, seed);
				int maxY = Math.Min(
					groundHeight - moduleLocation.Value.Y * moduleSize,
					moduleSize
				);

				maxMaxY = Math.Max(maxMaxY, maxY);

				for (int y = 0; y < maxY; y++)
				{
					ModuleGridPos inModulePos = new(new Vector3I(x, y, z));
					ConstructGridPos worldPos =
						inModulePos.ToConstruct(moduleLocation, moduleSize);

					int blockId = biome.GetBlockId(worldPos, groundHeight, seed);
					module.SetBlock(inModulePos, blockId);
				}
			}
		}

		if (maxMaxY > 0 && maxMaxY < moduleSize * 0.75f)
		{
			cachedMaxModuleY[
				new Vector2I(moduleLocation.Value.X, moduleLocation.Value.Z)
			] = moduleLocation.Value.Y;
		}
	}

	public override bool IsModuleNeeded(ModuleLocation moduleLocation)
	{
		return cachedMaxModuleY.GetValueOrDefault(
			new Vector2I(moduleLocation.Value.X, moduleLocation.Value.Z),
			int.MaxValue
		) >= moduleLocation.Value.Y;
	}
}