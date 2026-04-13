using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;

public partial class BiomeWorldGenerator : ConstructGenerator
{
	private int MAX_CACHE_SIZE = 100000;

	private List<Biome> biomes;
	private FastNoiseLite noise = new();

	private ConcurrentDictionary<Vector2I, int> cachedMaxModuleY = new ConcurrentDictionary<Vector2I, int>();

	private ConcurrentDictionary<Vector2I, int> groundHeightCache = new ConcurrentDictionary<Vector2I, int>();
	private ConcurrentQueue<Vector2I> groundHeightCacheQueue = new ConcurrentQueue<Vector2I>();
	private readonly object cacheLock = new object();

	public BiomeWorldGenerator(
		int seed,
		List<Biome> biomes
	) : base(seed)
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
		Module module = new Module();
		PopulateModule(module, moduleLocation);

		return new ModuleBlockGenerationResponse
		{
			GeneratedAllModules = false,
			GeneratedModules = new Dictionary<ModuleLocation, Module>
			{
				{ moduleLocation, module }
			},
		};
	}

	private void PopulateModule(Module module, ModuleLocation moduleLocation)
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

		int setSize = 0;
		for (int x = 0; x < moduleSize; x++)
		{
			int worldX = moduleOffsetX + x;

			for (int z = 0; z < moduleSize; z++)
			{
				int worldZ = moduleOffsetZ + z;

				inConstructLocation.X = worldX;
				inConstructLocation.Y = worldZ;

				int groundHeight = GetOrAddGroundHeight(inConstructLocation, biome);

				int maxY = Math.Min(groundHeight - moduleOffsetY, moduleSize);

				if (maxY <= 0)
					continue;
				if (maxY > maxMaxY)
					maxMaxY = maxY;

				worldPosVector.X = worldX;
				worldPosVector.Z = worldZ;

				for (int y = 0; y < maxY; y++)
				{
					worldPosVector.Y = moduleOffsetY + y;

					var block = biome.GetBlock(worldPos, groundHeight, seed);
					blockArray[setSize] = new BlockChange(new ModuleGridPos(new(x, y, z)), BlockChangeAction.PLACE, block);
					setSize += 1;
				}
			}
		}

		blockArray = blockArray[..setSize];
		module.SetBlocks(blockArray);

		if (maxMaxY > 0 && maxMaxY < moduleSize * 0.75f)
		{
			cachedMaxModuleY[new Vector2I(moduleLocation.Value.X, moduleLocation.Value.Z)] = moduleLocation.Value.Y;
		}
	}

	private int GetOrAddGroundHeight(Vector2I location, Biome biome)
	{
		if (groundHeightCache.TryGetValue(location, out int groundHeight))
		{
			return groundHeight;
		}

		lock (cacheLock)
		{
			if (groundHeightCache.TryGetValue(location, out groundHeight))
			{
				return groundHeight;
			}

			groundHeight = biome.GetGroundHeight(location, seed);

			Vector2I cacheKey = new Vector2I(location.X, location.Y);
			groundHeightCache[cacheKey] = groundHeight;
			groundHeightCacheQueue.Enqueue(cacheKey);

			while (groundHeightCacheQueue.Count > MAX_CACHE_SIZE)
			{
				if (groundHeightCacheQueue.TryDequeue(out Vector2I oldKey))
				{
					groundHeightCache.TryRemove(oldKey, out _);
				}
			}

			return groundHeight;
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