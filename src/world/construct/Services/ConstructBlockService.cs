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

    public void SetBlock(WorldGridPos worldPos, int blockId)
    {
        SetBlockInternal(worldPos, blockId);
        ModuleLocation moduleLoc = worldPos.ToModuleLocation(data.Transform, data.Modules.ModuleSize);
        UpdateModuleMesh(moduleLoc).FireAndForget();
    }

    public void SetBlocks(WorldGridPos[] worldPositions, int[] blockIds)
    {
        HashSet<ModuleLocation> moduleLocations = [];
        for (int i = 0; i < worldPositions.Length; i++)
        {
            WorldGridPos worldPos = worldPositions[i];
            int blockId = blockIds[i];
            ModuleLocation moduleLoc = worldPos.ToModuleLocation(data.Transform, data.Modules.ModuleSize);
            moduleLocations.Add(moduleLoc);
            SetBlockInternal(worldPos, blockId);
        }

        foreach (var moduleLoc in moduleLocations)
        {
            UpdateModuleMesh(moduleLoc).FireAndForget();
        }
    }

    public bool TryGetBlock(WorldGridPos worldPos, out int blockId)
    {
        ConstructGridPos conPos = worldPos.ToConstruct(data.Transform);
        return data.Modules.TryGetBlock(conPos, out blockId);
    }

    private void SetBlockInternal(WorldGridPos worldPos, int blockId)
    {
        ConstructGridPos conPos = worldPos.ToConstruct(data.Transform);

        data.Modules.SetBlock(conPos, blockId);

        if (blockId == -1)
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
