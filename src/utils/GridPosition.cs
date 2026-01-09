using Godot;

public readonly record struct WorldGridPos(Vector3I Value)
{
    public ConstructGridPos ToConstruct(WorldGridPos constructOffset)
    {
        return new(Value - constructOffset.Value);
    }

    public ModuleGridPos ToModule(WorldGridPos constructOffset, int moduleSize)
    {
        return ToConstruct(constructOffset).ToModule(moduleSize);
    }

    public ModuleLocation ToModuleLocation(WorldGridPos constructOffset, int moduleSize)
    {
        return ToConstruct(constructOffset).ToModuleLocation(moduleSize);
    }
}

public readonly record struct ConstructGridPos(Vector3I Value)
{

    public WorldGridPos ToWorld(WorldGridPos constructOffset)
    {
        return new(constructOffset.Value + Value);
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
    public WorldGridPos ToWorld(int moduleSize, WorldGridPos constructOffset)
    {
        return ToConstruct(moduleSize).ToWorld(constructOffset);
    }

    public ConstructGridPos ToConstruct(int moduleSize)
    {
        return new(Value * moduleSize);
    }
}


public readonly record struct ModuleGridPos(Vector3I Value)
{
    public WorldGridPos ToWorld(ModuleLocation moduleLocation, WorldGridPos constructOffset, int moduleSize)
    {
        return ToConstruct(moduleLocation, moduleSize).ToWorld(constructOffset);
    }

    public ConstructGridPos ToConstruct(ModuleLocation moduleLocation, int moduleSize)
    {
        return new(moduleLocation.Value * moduleSize + Value);
    }
}