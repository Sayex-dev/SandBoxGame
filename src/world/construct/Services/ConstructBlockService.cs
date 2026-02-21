using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public class ConstructBlockService
{
    private readonly ConstructData data;
    private readonly ConstructModuleBuilder moduleBuilder;
    private readonly ConstructVisualsController visuals;

    public ConstructBlockService(
        ConstructData data,
        ConstructModuleBuilder moduleBuilder,
        ConstructVisualsController visuals)
    {
        this.data = data;
        this.moduleBuilder = moduleBuilder;
        this.visuals = visuals;
    }

    public void SetBlock(WorldGridPos worldPos, Block block)
    {
        SetBlockInternal(worldPos, block);
        ModuleLocation moduleLoc = worldPos.ToModuleLocation(data.Transform, data.Modules.ModuleSize);
        UpdateModuleMesh(moduleLoc).FireAndForget();
    }

    public void SetBlocks(WorldGridPos[] worldPositions, Block[] blocks)
    {
        HashSet<ModuleLocation> moduleLocations = [];
        for (int i = 0; i < worldPositions.Length; i++)
        {
            WorldGridPos worldPos = worldPositions[i];
            ModuleLocation moduleLoc = worldPos.ToModuleLocation(data.Transform, data.Modules.ModuleSize);
            moduleLocations.Add(moduleLoc);
            SetBlockInternal(worldPos, blocks[i]);
        }

        foreach (var moduleLoc in moduleLocations)
        {
            UpdateModuleMesh(moduleLoc).FireAndForget();
        }
    }

    public bool TryGetBlock(WorldGridPos worldPos, out Block block)
    {
        ConstructGridPos conPos = worldPos.ToConstruct(data.Transform);
        return data.Modules.TryGetBlock(conPos, out block);
    }

    private void SetBlockInternal(WorldGridPos worldPos, Block block)
    {
        ConstructGridPos conPos = worldPos.ToConstruct(data.Transform);

        data.Modules.SetBlock(conPos, block);

        if (block.IsEmpty)
        {
            data.Bounds.RemovePosition(conPos, data.Modules.Modules);
        }
        else
        {
            data.Bounds.AddPosition(conPos);
        }
    }

    private async Task UpdateModuleMesh(ModuleLocation moduleLoc)
    {
        Module module;
        if (!data.Modules.TryGet(moduleLoc, out module))
            return;

        var context = new ModuleMeshGenerateContext(
            module,
            moduleLoc,
            data.BlockStore,
            data.ModuleMaterial
        );
        var mesh = await moduleBuilder.GenerateModuleMesh(context);
        visuals.RemoveModule(moduleLoc);
        visuals.AddModule(moduleLoc, mesh);
    }
}
