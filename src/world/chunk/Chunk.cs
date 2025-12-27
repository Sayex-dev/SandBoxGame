using Godot;
using System.Collections.Generic;

public partial class Chunk : MeshInstance3D
{
	public int ChunkSize { get; private set; }
	public int[] Blocks { get; private set; }
	public Dictionary<Vector3I, BlockState> BlockStates { get; private set; } = new();

	private Material _chunkMaterial;

	public Chunk(int chunkSize, Material chunkMaterial)
	{
		ChunkSize = chunkSize;
		_chunkMaterial = chunkMaterial;

		int blockCount = (int)Mathf.Pow(chunkSize, 3);
		Blocks = new int[blockCount];
		for (int i = 0; i < blockCount; i++)
			Blocks[i] = -1;
	}

	public int GetBlock(Vector3I chunkPos)
	{
		int index = ChunkToArrayPos(chunkPos);
		return Blocks[index];
	}

	public void SetBlock(Vector3I chunkPos, int blockId)
	{
		int index = ChunkToArrayPos(chunkPos);
		Blocks[index] = blockId;
	}

	public void SetBlockState(Vector3I chunkPos, BlockState blockState)
	{
		BlockStates[chunkPos] = blockState;
	}

	public bool HasBlockState(Vector3I chunkPos)
	{
		return BlockStates.ContainsKey(chunkPos);
	}

	public BlockState GetBlockState(Vector3I chunkPos)
	{
		return BlockStates[chunkPos];
	}

	public static Vector3I WorldToChunkPos(Vector3I worldPos, int chunkSize, Vector3I chunkLocation)
	{
		return worldPos - (chunkSize * chunkLocation);
	}

	public static Vector3I WrapToChunk(Vector3I pos, int chunkSize)
	{
		return new Vector3I(
			Mathf.PosMod(pos.X, chunkSize),
			Mathf.PosMod(pos.Y, chunkSize),
			Mathf.PosMod(pos.Z, chunkSize)
		);
	}

	public static Vector3I ChunkToWorldPos(Vector3I chunkLocation, Vector3I chunkSize, Vector3I chunkPos)
	{
		return (chunkLocation * chunkSize) + chunkPos;
	}

	public static Vector3I WorldToChunkLocation(Vector3I worldPos, int chunkSize)
	{
		return new Vector3I(
			Mathf.FloorToInt((float)worldPos.X / chunkSize),
			Mathf.FloorToInt((float)worldPos.Y / chunkSize),
			Mathf.FloorToInt((float)worldPos.Z / chunkSize)
		);
	}

	public int ChunkToArrayPos(Vector3I chunkPos)
	{
		return chunkPos.X
			 + chunkPos.Y * ChunkSize
			 + chunkPos.Z * ChunkSize * ChunkSize;
	}

	public Vector3I ArrayToChunkPos(int index)
	{
		int x = index % ChunkSize;
		int y = index / ChunkSize % ChunkSize;
		int z = index / (ChunkSize * ChunkSize);
		return new Vector3I(x, y, z);
	}

	public bool IsInChunk(Vector3I chunkPos)
	{
		bool correctX = chunkPos.X >= 0 && chunkPos.X < ChunkSize;
		bool correctY = chunkPos.Y >= 0 && chunkPos.Y < ChunkSize;
		bool correctZ = chunkPos.Z >= 0 && chunkPos.Z < ChunkSize;
		return correctX && correctY && correctZ;
	}

	public void BuildMesh(
		BlockStore blockStore,
		Chunk xPosChunk = null,
		Chunk xNegChunk = null,
		Chunk yPosChunk = null,
		Chunk yNegChunk = null,
		Chunk zPosChunk = null,
		Chunk zNegChunk = null)
	{
		Mesh = ChunkMeshGenerator.BuildChunkMesh(
			this,
			_chunkMaterial,
			blockStore,
			xPosChunk,
			xNegChunk,
			yPosChunk,
			yNegChunk,
			zPosChunk,
			zNegChunk
		);
	}
}