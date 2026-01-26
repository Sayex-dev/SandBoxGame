using System.Collections.Generic;

public abstract partial class ConstructGenerator
{
	protected int moduleSize;
	protected int seed;
	public ConstructGenerator(int moduleSize, int seed)
	{
		this.moduleSize = moduleSize;
		this.seed = seed;
	}

	public abstract ModuleBlockGenerationResponse GenerateModules(
		ModuleLocation moduleLocation,
		HashSet<ModuleLocation> prevLoaded = null
	);
	public abstract bool IsModuleNeeded(ModuleLocation moduleLocation);
}
