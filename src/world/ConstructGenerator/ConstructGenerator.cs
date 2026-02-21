using System.Collections.Generic;

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

	/// <summary>
	/// Returns all module locations that this generator requires.
	/// Used by one-time loaders to load all modules at once.
	/// Returns null if the generator has an unbounded/infinite set of modules (e.g. world terrain).
	/// </summary>
	public virtual HashSet<ModuleLocation> GetAllRequiredModules() => null;
}
