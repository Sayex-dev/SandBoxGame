using Godot;

public class ConstructData
{
    public ConstructTransform Transform { get; }
    public ConstructModuleController Modules { get; }
    public ConstructBoundsController Bounds { get; }
    public BlockStore BlockStore { get; }
    public Material ModuleMaterial { get; }

    public ConstructData(
        ConstructTransform transform,
        ConstructModuleController modules,
        ConstructBoundsController bounds,
        BlockStore blockStore,
        Material moduleMaterial)
    {
        Transform = transform;
        Modules = modules;
        Bounds = bounds;
        BlockStore = blockStore;
        ModuleMaterial = moduleMaterial;
    }
}
