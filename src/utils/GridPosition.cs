using Godot;

public readonly record struct WorldGridPos(Vector3I Value)
{
    public ConstructGridPos ToConstruct(ConstructTransform transform)
    {
        return new(Value - transform.WorldPos.Value);
    }

    public ModuleGridPos ToModule(ConstructTransform transform, int moduleSize)
    {
        return ToConstruct(transform).ToModule(moduleSize);
    }

    public ModuleLocation ToModuleLocation(ConstructTransform transform, int moduleSize)
    {
        return ToConstruct(transform).ToModuleLocation(moduleSize);
    }
}

public readonly record struct ConstructGridPos(Vector3I Value)
{

    public WorldGridPos ToWorld(ConstructTransform transform)
    {
        return new(transform.WorldPos.Value + Value);
    }

    public ModuleGridPos ToModule(int moduleSize)
    {
        Vector3I modulePos = new Vector3I(
            Mathf.PosMod(Value.X, moduleSize),
            Mathf.PosMod(Value.Y, moduleSize),
            Mathf.PosMod(Value.Z, moduleSize)
        );
        return new(modulePos);
    }

    public ModuleLocation ToModuleLocation(int moduleSize)
    {
        Vector3I moduleLocation = new Vector3I(
            Mathf.FloorToInt((float)Value.X / moduleSize),
            Mathf.FloorToInt((float)Value.Y / moduleSize),
            Mathf.FloorToInt((float)Value.Z / moduleSize)
        );
        return new(moduleLocation);
    }
}

public readonly record struct ModuleLocation(Vector3I Value)
{
    public WorldGridPos ToWorld(int moduleSize, ConstructTransform transform)
    {
        return ToConstruct(moduleSize).ToWorld(transform);
    }

    public ConstructGridPos ToConstruct(int moduleSize)
    {
        return new(Value * moduleSize);
    }
}


public readonly record struct ModuleGridPos(Vector3I Value)
{
    public WorldGridPos ToWorld(ModuleLocation moduleLocation, ConstructTransform transform, int moduleSize)
    {
        return ToConstruct(moduleLocation, moduleSize).ToWorld(transform);
    }

    public ConstructGridPos ToConstruct(ModuleLocation moduleLocation, int moduleSize)
    {
        return new(moduleLocation.Value * moduleSize + Value);
    }
}