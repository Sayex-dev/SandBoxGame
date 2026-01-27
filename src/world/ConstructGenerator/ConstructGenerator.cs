using System.Collections.Generic;
using Godot;

public abstract partial class ConstructGenerator : Node
{
	[Export] public int seed { get; private set; }
	protected int moduleSize;

	public void Initialize(int moduleSize)
	{
		this.moduleSize = moduleSize;
	}

	public abstract ModuleBlockGenerationResponse GenerateModules(
		ModuleLocation moduleLocation,
		HashSet<ModuleLocation> prevLoaded = null
	);
	public abstract bool IsModuleNeeded(ModuleLocation moduleLocation);
}
