using System.Collections.Generic;
using Godot;

public class GenerationResponse
{
	public bool generatedAllModules = false;
	public ModuleGridPos maxBlockPos;
	public ModuleGridPos minBlockPos;
	public Dictionary<ModuleLocation, Module> generatedModules = [];

}


public abstract partial class ConstructGenerator
{
	protected int moduleSize;
	protected int seed;
	public ConstructGenerator(int moduleSize, int seed)
	{
		this.moduleSize = moduleSize;
		this.seed = seed;
	}

	public abstract GenerationResponse GenerateModules(
		ModuleLocation moduleLocation,
		Material moduleMaterial,
		HashSet<ModuleLocation> prevLoaded = null
	);
	public abstract bool IsModuleNeeded(ModuleLocation moduleLocation);
}
