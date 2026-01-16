using System;
using System.Collections.Generic;
using Godot;


public class BiomeWorldGenerator : ConstructGenerator
{
	public Godot.Collections.Array<Biome> Biomes { get; }

	private FastNoiseLite _noise = new();
	private Dictionary<Vector2I, int> cachedMaxModuleY = [];

	public BiomeWorldGenerator(
		int moduleSize,
		int seed,
		Godot.Collections.Array<Biome> biomes
	) : base(moduleSize, seed)
	{
		Biomes = new Godot.Collections.Array<Biome>(biomes);

		_noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
		_noise.Seed = seed;

		for (int i = 0; i < Biomes.Count; i++)
		{
			Biomes[i].SetSeed(seed);
		}
	}

	public override GenerationResponse GenerateModules(
		ModuleLocation moduleLocation,
		Material moduleMaterial,
		HashSet<ModuleLocation> prevLoaded
	)
	{
		Module module = new Module(moduleSize, moduleMaterial);
		PopulateModule(module, moduleLocation, moduleSize);

		return new GenerationResponse
		{
			generatedAllModules = false,
			generatedModules = new Dictionary<ModuleLocation, Module>
			{
				{ moduleLocation, module }
			},
			maxBlockPos = new(Vector3I.One * (moduleSize - 1)),
			minBlockPos = new(Vector3I.Zero),
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
				Biome biome = Biomes[0]; // biome selection later
				Vector2I inConstructLocation =
					new(moduleOffset.Value.X + x, moduleOffset.Value.Z + z);

				int groundHeight = biome.GetGroundHeight(inConstructLocation);
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

					int blockId = biome.GetBlockId(worldPos, groundHeight);
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