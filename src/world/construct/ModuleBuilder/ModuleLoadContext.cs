using Godot;

public readonly struct ModuleLoadContext
{
    public int ModuleSize { get; }
    public BlockStore BlockStore { get; }
    public Material ModuleMaterial { get; }
    public ExposedSurfaceCache SurfaceCache { get; }
    public ConstructGenerator Generator { get; }

    public ModuleLoadContext(
        int moduleSize,
        BlockStore blockStore,
        Material moduleMaterial,
        ExposedSurfaceCache surfaceCache,
        ConstructGenerator generator)
    {
        ModuleSize = moduleSize;
        BlockStore = blockStore;
        ModuleMaterial = moduleMaterial;
        SurfaceCache = surfaceCache;
        Generator = generator;
    }
}