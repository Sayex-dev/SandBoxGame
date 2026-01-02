using Godot;
using System.Diagnostics;

[GlobalClass]
public abstract partial class WorldGenerator : Resource
{
	public abstract Module GenerateModules(Vector3I moduleLocation, Material moduleMaterial, int moduleSize);
	public abstract void SetSeed(int seed);
}
