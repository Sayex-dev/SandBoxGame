using Godot;
using System.Diagnostics;

[GlobalClass]
public abstract partial class WorldGenerator : Resource
{
	public abstract Chunk GenerateChunk(Vector3I chunkLocation, Material chunkMaterial, int chunkSize);
	public abstract void SetSeed(int seed);
}
