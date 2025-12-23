using Godot;

[GlobalClass]
public partial class SimpleNoiseWorldGenerator : WorldGenerator
{
	[Export] public Vector3I GenOffset { get; set; }
	[Export] public FastNoiseLite.NoiseTypeEnum NoiseType { get; set; } = FastNoiseLite.NoiseTypeEnum.Simplex;

	private FastNoiseLite _noise = new FastNoiseLite();

	public SimpleNoiseWorldGenerator()
	{
		_noise.NoiseType = NoiseType;
	}

	public override Chunk GenerateChunk(int seed, Vector3I chunkLocation, Material chunkMat, Vector3I chunkSize)
	{

		var chunk = new Chunk(chunkSize, chunkMat);
		SetGround(chunk, chunkLocation);
		return chunk;
	}

	private void SetGround(Chunk chunk, Vector3I chunkLocation)
	{
		var chunkSize = chunk.ChunkSize;

		for (int x = 0; x < chunkSize.X; x++)
		{
			for (int z = 0; z < chunkSize.Z; z++)
			{
				int xPos = chunkLocation.X * chunkSize.X + x + GenOffset.X;
				int zPos = chunkLocation.Z * chunkSize.Z + z + GenOffset.Z;

				int noiseHeight = (int)(_noise.GetNoise2D(xPos, zPos) * 10) + GenOffset.Y;

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
