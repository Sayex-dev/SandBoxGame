using Godot;

[GlobalClass]
public partial class LayeredWorldGenerator : WorldGenerator
{
	[Export] Godot.Collections.Array<Biome> Biomes;

	private FastNoiseLite _noise = new FastNoiseLite();

	public LayeredWorldGenerator()
	{
		_noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
	}

	public override Chunk GenerateChunk(int seed, Vector3I chunkLocation, Material chunkMaterial, Vector3I chunkSize)
	{
		Chunk chunk = new Chunk(chunkSize, chunkMaterial);
		PopulateChunk(seed, chunk, chunkLocation, chunkSize);
		return chunk;
	}

	private void PopulateChunk(int seed, Chunk chunk, Vector3I chunkLocation, Vector3I chunkSize)
	{
		Vector3I rootWorldPos = chunkLocation * chunkSize;
		for (int x = 0; x < chunkSize.X; x++)
		{
			for (int z = 0; z < chunkSize.Z; z++)
			{
				Vector2I worldPos = new Vector2I(rootWorldPos.X, rootWorldPos.Z) + new Vector2I(x, z);
				int noiseHeight = Biomes[0].GetGroundHeight(worldPos, seed);
				int maxY = Mathf.Min(noiseHeight - chunkLocation.Y * chunkSize.Y, chunkSize.Y);

				for (int y = 0; y < maxY; y++)
				{
					var inChunkPos = new Vector3I(x, y, z);
					chunk.SetBlock(inChunkPos, 1);
				}
			}
		}
	}
}
