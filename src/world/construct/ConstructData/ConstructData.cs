using Godot;

public class ConstructData
{
    public ConstructTransform Transform { get; }
    public ConstructModules Modules { get; }
    public ConstructBounds Bounds { get; }
    public Material ModuleMaterial { get; }

    public ConstructData(
        ConstructTransform transform,
        ConstructModules modules,
        ConstructBounds bounds,
        Material moduleMaterial)
    {
        Transform = transform;
        Modules = modules;
        Bounds = bounds;
        ModuleMaterial = moduleMaterial;
    }
}
