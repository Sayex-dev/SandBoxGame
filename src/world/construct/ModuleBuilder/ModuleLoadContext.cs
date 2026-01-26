using Godot;

public readonly struct ModuleLoadContext
{
    public int ModuleSize { get; }
    public BlockStore BlockStore { get; }
    public Material ModuleMaterial { get; }
    public ConstructGenerator Generator { get; }

    public ModuleLoadContext(
        int moduleSize,
        BlockStore blockStore,
        Material moduleMaterial,
        ConstructGenerator generator)
    {
        ModuleSize = moduleSize;
        BlockStore = blockStore;
        ModuleMaterial = moduleMaterial;
        Generator = generator;
    }
}