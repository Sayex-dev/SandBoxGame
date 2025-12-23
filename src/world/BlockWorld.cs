using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class BlockWorld : Node3D
{
	private Dictionary<Vector3I, Chunk> _chunks = new();
	private List<Vector3I> _queuedChunkPositions = new();

	private WorldGenerator _worldGen;
	private Vector3I _chunkSize;
	private Material _chunkMaterial;
	private AbilityManager _abilityManager;

	public BlockWorld(
		Vector3I chunkSize,
		WorldGenerator worldGen,
		Material chunkMaterial,
		AbilityManager abilityManager
	)
	{
		_chunkSize = chunkSize;
		_worldGen = worldGen;
		_chunkMaterial = chunkMaterial;
		_abilityManager = abilityManager;
	}

	public void SetBlockState(Vector3I worldPos, BlockState blockState)
	{
		var chunkLoc = Chunk.WorldToChunkLocation(worldPos, _chunkSize);
		var chunkPos = Chunk.WrapToChunk(worldPos, _chunkSize);
		_chunks[chunkLoc].SetBlockState(chunkPos, blockState);
	}

	public BlockState GetBlockState(Vector3I worldPos)
	{
		var chunkLoc = Chunk.WorldToChunkLocation(worldPos, _chunkSize);
		Chunk chunk = _chunks[chunkLoc];
		var chunkPos = Chunk.WorldToChunkPos(worldPos, chunk.ChunkSize, chunkLoc);
		return chunk.GetBlockState(chunkPos);
	}

	public bool HasBlockState(Vector3I worldPos)
	{
		var chunkLoc = Chunk.WorldToChunkLocation(worldPos, _chunkSize);
		var chunkPos = Chunk.WrapToChunk(worldPos, _chunkSize);
		return _chunks[chunkLoc].HasBlockState(chunkPos);
	}

	public void LoadPosition(Vector3 worldPos, Vector3I renderDistance)
	{
		var loadChunkPos = (Vector3I)(worldPos / (Vector3)_chunkSize).Floor();

		var desiredChunks = new List<Vector3I>();
		var addChunks = new List<Vector3I>();
		var removeChunks = new List<Vector3I>();

		for (int x = -renderDistance.X; x < renderDistance.X; x++)
		{
			for (int y = -renderDistance.Y; y < renderDistance.Y; y++)
			{
				for (int z = -renderDistance.Z; z < renderDistance.Z; z++)
				{
					desiredChunks.Add(loadChunkPos + new Vector3I(x, y, z));
				}
			}
		}

		foreach (var chunkPos in desiredChunks)
		{
			if (!_chunks.ContainsKey(chunkPos) && !_queuedChunkPositions.Contains(chunkPos))
			{
				_queuedChunkPositions.Add(chunkPos);
				addChunks.Add(chunkPos);
			}
		}

		foreach (var chunkPos in new List<Vector3I>(_chunks.Keys))
		{
			if (!desiredChunks.Contains(chunkPos))
				removeChunks.Add(chunkPos);

			if (_queuedChunkPositions.Contains(chunkPos))
				_queuedChunkPositions.Remove(chunkPos);
		}

		UpdateChunkLoading(addChunks, removeChunks);
	}

	public void UpdateChunkLoading(
		List<Vector3I> loadPositions = null,
		List<Vector3I> unloadPositions = null
	)
	{
		loadPositions ??= new List<Vector3I>();
		unloadPositions ??= new List<Vector3I>();

		// Unload immediately on main thread
		foreach (var chunkPos in unloadPositions)
		{
			if (_chunks.TryGetValue(chunkPos, out var chunk))
			{
				_chunks.Remove(chunkPos);
				chunk.QueueFree();
			}
		}

		// Spawn async chunk generation task
		_ = GenerateChunksAsync(loadPositions);
	}

	private async Task GenerateChunksAsync(List<Vector3I> loadPositions)
	{
		// Run heavy work in a background thread
		var generated = await Task.Run(() =>
		{
			var results = new Dictionary<Vector3I, Chunk>();
			foreach (var chunkPos in loadPositions)
			{
				var chunk = _worldGen.GenerateChunk(chunkPos, _chunkMaterial, _chunkSize);
				chunk.BuildMesh();
				chunk.Position = (Vector3)(_chunkSize * chunkPos);
				results[chunkPos] = chunk;
			}
			return results;
		});

		foreach (var kvp in generated)
		{
			var chunkPos = kvp.Key;
			var chunk = kvp.Value;

			if (_queuedChunkPositions.Contains(chunkPos))
			{
				_queuedChunkPositions.Remove(chunkPos);
				_chunks[chunkPos] = chunk;
				AddChild(chunk);
			}
			else
			{
				chunk.QueueFree();
			}
		}
	}
}