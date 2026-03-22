using Godot;

public readonly struct ModuleMeshGenerateContext
{
    public Module Module { get; }
    public ModuleLocation ModuleLocation { get; }
    public Material ModuleMaterial { get; }

    public ModuleMeshGenerateContext(
        Module module,
        ModuleLocation moduleLocation,
        Material moduleMaterial)
    {
        Module = module;
        ModuleLocation = moduleLocation;
        ModuleMaterial = moduleMaterial;
    }
}