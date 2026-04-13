using Godot;

public readonly record struct WorldGridPos(Vector3I Value)
{
    public ConstructGridPos ToConstruct(ConstructGridTransformData transform)
    {
        float radRot = Mathf.DegToRad(transform.YRotation);
        return new((Vector3I)((Vector3)(Value - transform.WorldPos.Value)).Rotated(Vector3.Up, -radRot));
    }

    public ModuleGridPos ToModule(ConstructGridTransformData transform)
    {
        return ToConstruct(transform).ToModule();
    }

    public ModuleLocation ToModuleLocation(ConstructGridTransformData transform)
    {
        return ToConstruct(transform).ToModuleLocation();
    }

    public static implicit operator WorldGridPos(Vector3I value)
    {
        return new(value);
    }

    public static implicit operator Vector3I(WorldGridPos value)
    {
        return value.Value;
    }
}

public readonly record struct ConstructGridPos(Vector3I Value)
{

    public WorldGridPos ToWorld(ConstructGridTransformData transform)
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

    public static implicit operator ConstructGridPos(Vector3I value)
    {
        return new(value);
    }

    public static implicit operator Vector3I(ConstructGridPos value)
    {
        return value.Value;
    }
}

public readonly record struct ModuleLocation(Vector3I Value)
{
    public WorldGridPos ToWorld(int moduleSize, ConstructGridTransformData transform)
    {
        return ToConstruct(moduleSize).ToWorld(transform);
    }

    public ConstructGridPos ToConstruct(int moduleSize)
    {
        return new(Value * moduleSize);
    }

    public static implicit operator ModuleLocation(Vector3I value)
    {
        return new(value);
    }

    public static implicit operator Vector3I(ModuleLocation value)
    {
        return value.Value;
    }
}


public readonly record struct ModuleGridPos(Vector3I Value)
{
    public WorldGridPos ToWorld(ModuleLocation moduleLocation, ConstructGridTransformData transform)
    {
        return ToConstruct(moduleLocation).ToWorld(transform);
    }

    public ConstructGridPos ToConstruct(ModuleLocation moduleLocation)
    {
        int moduleSize = GameSettings.Instance.ModuleSize;
        return new(moduleLocation.Value * moduleSize + Value);
    }

    public static implicit operator ModuleGridPos(Vector3I value)
    {
        return new(value);
    }

    public static implicit operator Vector3I(ModuleGridPos value)
    {
        return value.Value;
    }
}