using Godot;

public class ConstructData
{
    public ConstructPhysicsData PhysicsData { get; }
    public ConstructGridTransform GridTransform { get; }
    public ConstructModules Modules { get; }
    public ConstructBounds Bounds { get; }
    public Material ModuleMaterial { get; }

    public ConstructData(
        ConstructPhysicsData physicsData,
        ConstructGridTransform transform,
        ConstructModules modules,
        ConstructBounds bounds,
        Material moduleMaterial)
    {
        PhysicsData = physicsData;
        GridTransform = transform;
        Modules = modules;
        Bounds = bounds;
        ModuleMaterial = moduleMaterial;
    }
}
