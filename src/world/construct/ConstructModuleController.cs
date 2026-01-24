using System;
using System.Collections.Generic;

public class ConstructModuleController
{
    public int ModuleSize { get; }

    public readonly Dictionary<ModuleLocation, Module> Modules = new();

    public ConstructModuleController(int moduleSize)
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
        out int blockId
    )
    {
        blockId = -1;

        ModuleLocation moduleLoc = conPos.ToModuleLocation(ModuleSize);
        if (!Modules.TryGetValue(moduleLoc, out var module))
            return false;

        ModuleGridPos inModule = conPos.ToModule(ModuleSize);
        if (!module.HasBlock(inModule))
            return false;

        blockId = module.GetBlock(inModule);
        return blockId != -1;
    }

    public void SetBlock(
        ConstructGridPos conPos,
        int blockId
    )
    {
        ModuleLocation moduleLoc = conPos.ToModuleLocation(ModuleSize);
        if (!Modules.TryGetValue(moduleLoc, out var module))
        {
            module = new Module(ModuleSize);
            Add(moduleLoc, module);
        }

        ModuleGridPos inModule = conPos.ToModule(ModuleSize);
        module.SetBlock(inModule, blockId);
        if (!module.HasBlocks) Modules.Remove(moduleLoc);
    }
}