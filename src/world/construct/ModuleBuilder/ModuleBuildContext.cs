using Godot;

public readonly struct ModuleBuildContext
{
    public int ModuleSize { get; }
    public Material ModuleMaterial { get; }
    public ConstructGenerator Generator { get; }

    public ModuleBuildContext(
        int moduleSize,
        Material moduleMaterial,
        ConstructGenerator generator)
    {
        ModuleSize = moduleSize;
        ModuleMaterial = moduleMaterial;
        Generator = generator;
    }
}