using Godot;

public class ConstructData
{
    public ConstructPhysicsData PhysicsData {get;}
    public ConstructTransform Transform { get; }
    public ConstructModules Modules { get; }
    public ConstructBounds Bounds { get; }
    public Material ModuleMaterial { get; }

    public ConstructData(
        ConstructPhysicsData physicsData,
        ConstructTransform transform,
        ConstructModules modules,
        ConstructBounds bounds,
        Material moduleMaterial)
    {
        PhysicsData = physicsData;
        Transform = transform;
        Modules = modules;
        Bounds = bounds;
        ModuleMaterial = moduleMaterial;
    }
}
