using System;
using System.Collections.Generic;

public class ConstructBlockController
{
    public event Action<BlockChange<ConstructGridPos>> OnBlockChanged;

    public event Action<ModuleLocation, BlockChange<ModuleGridPos>[]> OnModuleChanged;
    public event Action<ModuleLocation, Module> OnModuleAdded;
    public event Action<ModuleLocation, Module> OnModuleRemoved;
    public bool FullyLoaded { get; set; }

    public readonly Dictionary<ModuleLocation, Module> Modules = new();

    public void AddModule(ModuleLocation location, Module module)
    {
        Modules[location] = module;
        module.OnModuleChanged += (blockChanges) => OnModuleChanged(location, blockChanges);
        OnModuleAdded?.Invoke(location, module);
    }

    public bool RemoveModule(ModuleLocation location, out Module module)
    {
        if (!Modules.TryGetValue(location, out module))
            return false;

        module.OnModuleChanged -= (blockChanges) => OnModuleChanged(location, blockChanges);
        Modules.Remove(location);
        OnModuleRemoved?.Invoke(location, module);
        return true;
    }

    public bool TryGetModule(ModuleLocation location, out Module module)
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
            AddModule(moduleLoc, module);
        }

        OnBlockChanged?.Invoke(new BlockChange<ConstructGridPos>(pos, BlockChangeAction.PLACE, block));

        ModuleGridPos inModule = pos.ToModule();
        module.SetBlock(inModule, block);
        if (!module.HasBlocks)
            Modules.Remove(moduleLoc);
    }

    public bool TryRemoveBlock(
        ConstructGridPos pos,
        out Block block
    )
    {
        block = default;

        ModuleLocation moduleLoc = pos.ToModuleLocation();
        if (Modules.TryGetValue(moduleLoc, out var module))
        {
            block = module.GetBlock(pos.ToModule());
            return true;
        }
        else
        {
            return false;
        }
    }
}