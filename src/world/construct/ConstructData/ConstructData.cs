using Godot;

public class ConstructData
{
    public ConstructPhysicsData PhysicsData { get; }
    public ConstructGridTransform GridTransform { get; }
    public ConstructModules Modules { get; }
    public ConstructBounds Bounds { get; }

    public ConstructData(
        ConstructPhysicsData physicsData,
        ConstructGridTransform transform,
        ConstructModules modules,
        ConstructBounds bounds)
    {
        PhysicsData = physicsData;
        GridTransform = transform;
        Modules = modules;
        Bounds = bounds;
    }
}
