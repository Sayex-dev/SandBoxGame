using System.Collections.Generic;
using Godot;

public abstract partial class ConstructGenerator
{
	protected int seed;
	protected int moduleSize;

	public ConstructGenerator(int moduleSize, int seed)
	{
		this.seed = seed;
		this.moduleSize = moduleSize;
	}

	public abstract ModuleBlockGenerationResponse GenerateModules(
		ModuleLocation moduleLocation,
		HashSet<ModuleLocation> prevLoaded = null
	);
	public abstract bool IsModuleNeeded(ModuleLocation moduleLocation);
}
