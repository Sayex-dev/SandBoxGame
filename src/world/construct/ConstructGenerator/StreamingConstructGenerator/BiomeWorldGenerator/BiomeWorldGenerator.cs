using System;
using System.Collections.Generic;
using System.Linq;
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

		int moduleOffsetX = moduleLocation.Value.X * moduleSize;
		int moduleOffsetY = moduleLocation.Value.Y * moduleSize;
		int moduleOffsetZ = moduleLocation.Value.Z * moduleSize;

		int moduleSize2 = moduleSize * moduleSize;
		int moduleSize3 = moduleSize2 * moduleSize;
		BlockChange[] blockArray = new BlockChange[moduleSize3];

		Biome biome = biomes[0];

		Vector2I inConstructLocation = new Vector2I();
		Vector3I worldPosVector = new Vector3I();
		ConstructGridPos worldPos = new ConstructGridPos(worldPosVector);

		for (int x = 0; x < moduleSize; x++)
		{
			int worldX = moduleOffsetX + x;

			for (int z = 0; z < moduleSize; z++)
			{
				int worldZ = moduleOffsetZ + z;

				// Reuse vector instead of allocating
				inConstructLocation.X = worldX;
				inConstructLocation.Y = worldZ;

				int groundHeight = biome.GetGroundHeight(inConstructLocation, seed);
				int maxY = Math.Min(groundHeight - moduleOffsetY, moduleSize);

				if (maxY <= 0)
					continue;
				if (maxY > maxMaxY)
					maxMaxY = maxY;

				// Pre-calculate base array index for this XZ column
				int columnBaseIndex = x + z * moduleSize2;

				// Critical optimization: Calculate world position directly
				worldPosVector.X = worldX;
				worldPosVector.Z = worldZ;

				for (int y = 0; y < maxY; y++)
				{
					// Only update Y component (X and Z are constant for this column)
					worldPosVector.Y = moduleOffsetY + y;

					var block = biome.GetBlock(worldPos, groundHeight, seed);
					blockArray[columnBaseIndex + y * moduleSize] = new BlockChange(BlockChangeAction.REPLACE, block);
				}
			}
		}

		// Apply all blocks at once
		module.SetAllBlocks(blockArray);

		if (maxMaxY > 0 && maxMaxY < moduleSize * 0.75f)
		{
			cachedMaxModuleY[new Vector2I(moduleLocation.Value.X, moduleLocation.Value.Z)] = moduleLocation.Value.Y;
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