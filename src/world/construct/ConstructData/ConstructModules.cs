using System.Collections.Generic;

public partial class ConstructModules
{
    public bool FullyLoaded { get; private set; }
    public int ModuleSize { get; private set; }

    public readonly Dictionary<ModuleLocation, Module> Modules = new();

    public ConstructModules(int moduleSize)
    {
        ModuleSize = moduleSize;
    }

    public void Add(ModuleLocation location, Module module)
    {
        Modules[location] = module;
    }

    public bool Remove(ModuleLocation location, out Module module)
    {
        if (!Modules.TryGetValue(location, out module))
            return false;

        Modules.Remove(location);
        return true;
    }

    public bool TryGet(ModuleLocation location, out Module module)
    {
        return Modules.TryGetValue(location, out module);
    }

    public IReadOnlyDictionary<ModuleLocation, Module> All => Modules;

    public bool TryGetBlock(
        ConstructGridPos conPos,
        out Block block
    )
    {
        block = default;

        ModuleLocation moduleLoc = conPos.ToModuleLocation(ModuleSize);
        if (!Modules.TryGetValue(moduleLoc, out var module))
            return false;

        ModuleGridPos inModule = conPos.ToModule(ModuleSize);
        if (!module.HasBlock(inModule, out block))
            return false;

        return !block.IsEmpty;
    }

    public void SetBlock(
        ConstructGridPos conPos,
        Block block
    )
    {
        ModuleLocation moduleLoc = conPos.ToModuleLocation(ModuleSize);
        if (!Modules.TryGetValue(moduleLoc, out var module))
        {
            module = new Module(ModuleSize);
            Add(moduleLoc, module);
        }

        ModuleGridPos inModule = conPos.ToModule(ModuleSize);
        module.SetBlock(inModule, block);
        if (!module.HasBlocks)
            Modules.Remove(moduleLoc);
    }
}