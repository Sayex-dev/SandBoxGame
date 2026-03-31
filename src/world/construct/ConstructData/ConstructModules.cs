using System.Collections.Generic;

public partial class ConstructModules
{
    public bool FullyLoaded { get; set; }

    public readonly Dictionary<ModuleLocation, Module> Modules = new();


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
        ConstructGridPos pos,
        out Block block
    )
    {
        block = default;

        ModuleLocation moduleLoc = pos.ToModuleLocation();
        if (!Modules.TryGetValue(moduleLoc, out var module))
            return false;

        ModuleGridPos inModule = pos.ToModule();
        if (!module.HasBlock(inModule, out block))
            return false;

        return !block.IsEmpty;
    }

    public void SetBlock(
        ConstructGridPos pos,
        Block block
    )
    {
        ModuleLocation moduleLoc = pos.ToModuleLocation();
        if (!Modules.TryGetValue(moduleLoc, out var module))
        {
            module = new Module();
            Add(moduleLoc, module);
        }

        ModuleGridPos inModule = pos.ToModule();
        module.SetBlock(inModule, block);
        if (!module.HasBlocks)
            Modules.Remove(moduleLoc);
    }
}