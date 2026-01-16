using System;
using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class BiomeWorldGenerator : ConstructGenerator
{
	[Export] Godot.Collections.Array<Biome> Biomes;

	private FastNoiseLite _noise = new FastNoiseLite();
	private int maxY = 0;
	private Dictionary<Vector2I, int> cachedMaxModuleY = [];

	public BiomeWorldGenerator()
	{
		_noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
	}

	public override void Init(int moduleSize, int seed)
	{
		base.Init(moduleSize, seed);
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
		Module module = new Module(ModuleSize, moduleMaterial);
		PopulateModule(module, moduleLocation, ModuleSize);
		return new GenerationResponse
		{
			generatedAllModules = false,
			generatedModules = new Dictionary<ModuleLocation, Module>
			{
				{moduleLocation, module}
			},
			maxBlockPos = new(Vector3I.One * (ModuleSize - 1)),
			minBlockPos = new(Vector3I.Zero),
		};
	}

	private void PopulateModule(Module module, ModuleLocation moduleLocation, int moduleSize)
	{
		int maxMaxY = 0;
		ConstructGridPos moduleOffset = moduleLocation.ToConstruct(ModuleSize);
		for (int x = 0; x < moduleSize; x++)
		{
			for (int z = 0; z < moduleSize; z++)
			{
				Biome biome = Biomes[0];
				Vector2I inConstructLocation = new Vector2I(moduleOffset.Value.X, moduleOffset.Value.Z) + new Vector2I(x, z);
				int groundHeight = biome.GetGroundHeight(inConstructLocation);
				int maxY = Math.Min(groundHeight - moduleLocation.Value.Y * moduleSize, moduleSize);
				maxMaxY = Math.Max(maxMaxY, maxY);

				for (int y = 0; y < maxY; y++)
				{
					ModuleGridPos inModulePos = new(new Vector3I(x, y, z));
					ConstructGridPos worldPos = inModulePos.ToConstruct(moduleLocation, ModuleSize);
					int blockId = biome.GetBlockId(worldPos, groundHeight);
					module.SetBlock(inModulePos, blockId);
				}
			}
		}

		if (maxMaxY > 0 && maxMaxY < moduleSize * 0.75)
		{
			cachedMaxModuleY[new Vector2I(moduleLocation.Value.X, moduleLocation.Value.Z)] = moduleLocation.Value.Y;
		}
	}

	public override bool IsModuleNeeded(ModuleLocation moduleLocation)
	{
		return cachedMaxModuleY.GetValueOrDefault(
			new Vector2I(moduleLocation.Value.X, moduleLocation.Value.Z), int.MaxValue) >= moduleLocation.Value.Y;
	}
}
