using System.Collections.Generic;
using Godot;

public class ModuleGenerationResponse
{
	public bool GeneratedAllModules = false;
	public ConstructBoundsController bounds;
	public ExposedSurfaceCache cache;
	public Dictionary<ModuleLocation, Module> GeneratedModules = [];
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

	public abstract ModuleGenerationResponse GenerateModules(
		ModuleLocation moduleLocation,
		HashSet<ModuleLocation> prevLoaded = null
	);
	public abstract bool IsModuleNeeded(ModuleLocation moduleLocation);
}
