using Godot;

public interface IGridPos
{
    Vector3I Value { get; }
}

/// <summary>
/// Integer cell position in the global/world grid space.
/// This is an absolute grid coordinate using world axes. Converting to a construct
/// space subtracts the construct world origin and un-rotates by the construct Y rotation.
/// </summary>
public readonly record struct WorldGridPos(Vector3I Value) : IGridPos
{
    public ConstructGridPos ToConstruct(ConstructGridTransformController transform)
    {
        float radRot = Mathf.DegToRad(transform.YRotation);
        return new((Vector3I)((Vector3)(Value - transform.WorldPos.Value)).Rotated(Vector3.Up, -radRot));
    }

    public ModuleGridPos ToModule(ConstructGridTransformController transform)
    {
        return ToConstruct(transform).ToModule();
    }

    public ModuleLocation ToModuleLocation(ConstructGridTransformController transform)
    {
        return ToConstruct(transform).ToModuleLocation();
    }

    public static explicit operator WorldGridPos(Vector3I value) => new(value);
    public static implicit operator Vector3I(WorldGridPos value) => value.Value;
}

/// <summary>
/// Integer cell position in a construct's local grid space.
/// The origin is the construct's world grid position, and the axes are local to the
/// construct before applying its world Y rotation. This space has the same cell size
/// as the world grid, but is relative to one construct.
/// </summary>
public readonly record struct ConstructGridPos(Vector3I Value) : IGridPos
{

    public WorldGridPos ToWorld(ConstructGridTransformController transform)
    {
        Vector3I rotated = (Vector3I)((Vector3)Value).Rotated(Vector3.Up, transform.YRotation).Round();
        return new(transform.WorldPos.Value + rotated);
    }

    public ModuleGridPos ToModule()
    {
        int moduleSize = GameSettings.Instance.ModuleSize;

        Vector3I modulePos = new Vector3I(
            Mathf.PosMod(Value.X, moduleSize),
            Mathf.PosMod(Value.Y, moduleSize),
            Mathf.PosMod(Value.Z, moduleSize)
        );
        return new(modulePos);
    }

    public ModuleLocation ToModuleLocation()
    {
        int moduleSize = GameSettings.Instance.ModuleSize;

        Vector3I moduleLocation = new Vector3I(
            Mathf.FloorToInt((float)Value.X / moduleSize),
            Mathf.FloorToInt((float)Value.Y / moduleSize),
            Mathf.FloorToInt((float)Value.Z / moduleSize)
        );
        return new(moduleLocation);
    }

    public static explicit operator ConstructGridPos(Vector3I value) => new(value);
    public static implicit operator Vector3I(ConstructGridPos value) => value.Value;
}

/// <summary>
/// Integer module/chunk coordinate within a construct's local grid space.
/// Each step in this space represents one whole module of size ModuleSize.
/// For example, ModuleLocation (1, 0, 0) starts at construct-grid cell
/// (ModuleSize, 0, 0). Negative positions use floor division semantics.
/// </summary>
public readonly record struct ModuleLocation(Vector3I Value) : IGridPos
{
    public WorldGridPos ToWorld(int moduleSize, ConstructGridTransformController transform)
    {
        return ToConstruct(moduleSize).ToWorld(transform);
    }

    public ConstructGridPos ToConstruct(int moduleSize)
    {
        return new(Value * moduleSize);
    }

    public static explicit operator ModuleLocation(Vector3I value) => new(value);
    public static implicit operator Vector3I(ModuleLocation value) => value.Value;
}

/// <summary>
/// Integer cell offset inside a single module.
/// Each component is the local position within that module, normally in the range
/// [0, ModuleSize - 1]. This position is not globally unique without a
/// corresponding ModuleLocation.
/// </summary>
public readonly record struct ModuleGridPos(Vector3I Value) : IGridPos
{
    public WorldGridPos ToWorld(ModuleLocation moduleLocation, ConstructGridTransformController transform)
    {
        return ToConstruct(moduleLocation).ToWorld(transform);
    }

    public ConstructGridPos ToConstruct(ModuleLocation moduleLocation)
    {
        int moduleSize = GameSettings.Instance.ModuleSize;
        return new(moduleLocation.Value * moduleSize + Value);
    }

    public static explicit operator ModuleGridPos(Vector3I value) => new(value);
    public static implicit operator Vector3I(ModuleGridPos value) => value.Value;
}
