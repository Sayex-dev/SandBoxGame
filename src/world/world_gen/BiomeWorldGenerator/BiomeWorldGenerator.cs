using Godot;

[GlobalClass]
public partial class BiomeWorldGenerator : WorldGenerator
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

	public override Chunk GenerateChunk(Vector3I chunkLocation, Material chunkMaterial, int chunkSize)
	{
		Chunk chunk = new Chunk(chunkSize, chunkMaterial);
		PopulateChunk(chunk, chunkLocation, chunkSize);
		return chunk;
	}

	private void PopulateChunk(Chunk chunk, Vector3I chunkLocation, int chunkSize)
	{
		Vector3I rootWorldPos = chunkLocation * chunkSize;
		for (int x = 0; x < chunkSize; x++)
		{
			for (int z = 0; z < chunkSize; z++)
			{
				Biome biome = Biomes[0];
				Vector2I worldLocation = new Vector2I(rootWorldPos.X, rootWorldPos.Z) + new Vector2I(x, z);
				int groundHeight = biome.GetGroundHeight(worldLocation);
				int maxY = Mathf.Min(groundHeight - chunkLocation.Y * chunkSize, chunkSize);

				for (int y = 0; y < maxY; y++)
				{
					Vector3I inChunkPos = new Vector3I(x, y, z);
					Vector3I worldPos = inChunkPos + rootWorldPos;
					int blockId = biome.GetBlockId(worldPos, groundHeight);
					chunk.SetBlock(inChunkPos, blockId);
				}
			}
		}
	}
}
