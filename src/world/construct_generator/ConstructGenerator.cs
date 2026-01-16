using System.Collections.Generic;
using Godot;

public class GenerationResponse
{
	public bool generatedAllModules = false;
	public ModuleGridPos maxBlockPos;
	public ModuleGridPos minBlockPos;
	public Dictionary<ModuleLocation, Module> generatedModules = [];

}

[GlobalClass]
public abstract partial class ConstructGenerator : Resource
{
	public int ModuleSize;
	public int Seed;

	public virtual void Init(int moduleSize, int seed)
	{
		this.ModuleSize = moduleSize;
		this.Seed = seed;
	}
	public abstract GenerationResponse GenerateModules(
		ModuleLocation moduleLocation,
		Material moduleMaterial,
		HashSet<ModuleLocation> prevLoaded = null
	);
	public abstract bool IsModuleNeeded(ModuleLocation moduleLocation);
}
