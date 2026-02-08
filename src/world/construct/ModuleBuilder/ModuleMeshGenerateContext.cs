using Godot;

public readonly struct ModuleMeshGenerateContext
{
    public Module Module { get; }
    public ModuleLocation ModuleLocation { get; }
    public BlockStore BlockStore { get; }
    public Material ModuleMaterial { get; }

    public ModuleMeshGenerateContext(
        Module module,
        ModuleLocation moduleLocation,
        BlockStore blockStore,
        Material moduleMaterial)
    {
        Module = module;
        ModuleLocation = moduleLocation;
        BlockStore = blockStore;
        ModuleMaterial = moduleMaterial;
    }
}