using Godot;

public readonly struct ModuleBuildContext
{
    public Material ModuleMaterial { get; }
    public ConstructGenerator Generator { get; }

    public ModuleBuildContext(
        Material moduleMaterial,
        ConstructGenerator generator)
    {
        ModuleMaterial = moduleMaterial;
        Generator = generator;
    }
}