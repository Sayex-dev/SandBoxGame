using Godot;
using System.Diagnostics;

[GlobalClass]
public abstract partial class WorldGenerator : Resource
{
	public abstract Chunk GenerateChunk(int seed, Vector3I chunkLocation, Material chunkMaterial, Vector3I chunkSize);
}
