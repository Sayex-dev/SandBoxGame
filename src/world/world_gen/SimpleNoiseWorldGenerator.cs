using Godot;

[GlobalClass]
public partial class SimpleNoiseWorldGenerator : WorldGenerator
{
	[Export] public Vector3I GenOffset { get; set; }
	[Export] public FastNoiseLite.NoiseTypeEnum NoiseType { get; set; } = FastNoiseLite.NoiseTypeEnum.Simplex;
	[Export] public float NoiseScale { get; set; } = 1;
	[Export] public float HeightScale { get; set; } = 10;

	private FastNoiseLite _noise = new FastNoiseLite();

	public SimpleNoiseWorldGenerator()
	{
		_noise.NoiseType = NoiseType;
	}

	public override Chunk GenerateChunk(Vector3I chunkLocation, Material chunkMat, int chunkSize)
	{

		var chunk = new Chunk(chunkSize, chunkMat);
		SetGround(chunk, chunkLocation);
		return chunk;
	}

	private void SetGround(Chunk chunk, Vector3I chunkLocation)
	{
		var chunkSize = chunk.ChunkSize;

		for (int x = 0; x < chunkSize; x++)
		{
			for (int z = 0; z < chunkSize; z++)
			{
				float xPos = (chunkLocation.X * chunkSize + x + GenOffset.X) * NoiseScale;
				float zPos = (chunkLocation.Z * chunkSize + z + GenOffset.Z) * NoiseScale;

				int noiseHeight = (int)(_noise.GetNoise2D(xPos, zPos) * HeightScale) + GenOffset.Y;

				int maxY = Mathf.Min(noiseHeight - chunkLocation.Y * chunkSize, chunkSize);

				for (int y = 0; y < maxY; y++)
				{
					var inChunkPos = new Vector3I(x, y, z);
					chunk.SetBlock(inChunkPos, 1);
				}
			}
		}
	}

	public override void SetSeed(int seed)
	{
		_noise.Seed = seed;
	}

}
