using Godot;

public readonly struct ModuleMeshGenerateContext
{
    public Module Module { get; }
    public ModuleLocation ModuleLocation { get; }

    public ModuleMeshGenerateContext(
        Module module,
        ModuleLocation moduleLocation)
    {
        Module = module;
        ModuleLocation = moduleLocation;
    }
}