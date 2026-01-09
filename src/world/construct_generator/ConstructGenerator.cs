using System.Collections.Generic;
using Godot;

public class GenerationResponse
{
	public bool generatedAllModules = false;
	public Dictionary<ModuleLocation, Module> generatedModules = [];

}

[GlobalClass]
public abstract partial class ConstructGenerator : Resource
{
	public int ModuleSize;

	public virtual void Init(int moduleSize)
	{
		this.ModuleSize = moduleSize;
	}
	public abstract GenerationResponse GenerateModules(
		ModuleLocation moduleLocation,
		Material moduleMaterial,
		HashSet<ModuleLocation> prevLoaded = null
	);
	public abstract bool IsModuleNeeded(ModuleLocation moduleLocation);
	public abstract void SetSeed(int seed);
}
