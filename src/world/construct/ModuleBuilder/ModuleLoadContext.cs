using Godot;

public readonly struct ModuleLoadContext
{
    public int ModuleSize { get; }
    public Material ModuleMaterial { get; }
    public ConstructGenerator Generator { get; }

    public ModuleLoadContext(
        int moduleSize,
        Material moduleMaterial,
        ConstructGenerator generator)
    {
        ModuleSize = moduleSize;
        ModuleMaterial = moduleMaterial;
        Generator = generator;
    }
}