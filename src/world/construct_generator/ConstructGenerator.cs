using System.Collections.Generic;
using Godot;

public class GenerationResponse
{
	public bool generatedAllModules = false;
	public Dictionary<Vector3I, Module> generatedModules = [];

}

[GlobalClass]
public abstract partial class ConstructGenerator : Resource
{
	public int moduleSize;

	public virtual void Init(int moduleSize)
	{
		this.moduleSize = moduleSize;
	}
	public abstract GenerationResponse GenerateModules(
		Vector3I relativeWorldPos,
		Material moduleMaterial,
		HashSet<Vector3I> prevLoaded = null
	);
	public abstract bool IsModuleNeeded(Vector3I chunkLocation);
	public abstract void SetSeed(int seed);
}
