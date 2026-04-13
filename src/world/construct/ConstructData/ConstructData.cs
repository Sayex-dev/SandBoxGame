using Godot;

public class ConstructData
{
    public ConstructPhysicsData PhysicsData { get; }
    public ConstructGridTransformData GridTransform { get; }
    public ConstructModulesData Modules { get; }
    public ConstructBoundsData Bounds { get; }

    public ConstructData(
        ConstructPhysicsData physicsData,
        ConstructGridTransformData transform,
        ConstructModulesData modules,
        ConstructBoundsData bounds)
    {
        PhysicsData = physicsData;
        GridTransform = transform;
        Modules = modules;
        Bounds = bounds;
    }
}
