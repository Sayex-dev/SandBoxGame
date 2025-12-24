using Godot;

[GlobalClass]
public partial class PresetWorldGenerator : WorldGenerator
{
	[Export] public Godot.Collections.Array<Vector4I> Blocks { get; set; }
	[Export] public Vector3I Offset { get; set; }

	public PresetWorldGenerator()
	{
	}

	public override Chunk GenerateChunk(Vector3I chunkLocation, Material chunkMaterial, Vector3I chunkSize)
	{
		var chunk = new Chunk(chunkSize, chunkMaterial);

		foreach (var block in Blocks)
		{
			var worldPos = new Vector3I(block.X, block.Y, block.Z) + Offset;
			var inChunkPos = Chunk.WorldToChunkPos(worldPos, chunkSize, chunkLocation);

			if (chunk.IsInChunk(inChunkPos))
			{
				chunk.SetBlock(inChunkPos, block.W);
			}
		}

		return chunk;
	}

	public override void SetSeed(int seed)
	{
	}
}
