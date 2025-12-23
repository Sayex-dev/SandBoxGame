using Godot;

public partial class LayeredWorldGenerator : WorldGenerator
{
	[Export] float PillarDensity { get; set; } = 1;

	private FastNoiseLite _noise = new FastNoiseLite();

	public LayeredWorldGenerator()
	{
		_noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
	}

	public override Chunk GenerateChunk(int seed, Vector3I chunkLocation, Material chunkMaterial, Vector3I chunkSize)
	{
		Chunk chunk = new Chunk(chunkSize, chunkMaterial);
		PopulateChunk(chunk, chunkLocation);
		return chunk;
	}

	private void PopulateChunk(Chunk chunk, Vector3I chunkLocation)
	{

	}

}
