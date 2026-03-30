using System.Collections.Generic;

public class ConstructBlockService
{
    private readonly ConstructData data;

    public ConstructBlockService(
        ConstructData data)
    {
        this.data = data;
    }

    public void SetBlock(WorldGridPos pos, Block block)
    {
        ConstructGridPos conPos = pos.ToConstruct(data.GridTransform);
        SetBlock(conPos, block);
    }

    public void SetBlock(ConstructGridPos pos, Block block)
    {
        SetBlockInternal(pos, block);
    }

    public void SetBlocks(ConstructGridPos[] positions, Block[] blocks)
    {
        HashSet<ModuleLocation> moduleLocations = [];
        for (int i = 0; i < positions.Length; i++)
        {
            ConstructGridPos pos = positions[i];
            ModuleLocation moduleLoc = pos.ToModuleLocation(data.Modules.ModuleSize);
            moduleLocations.Add(moduleLoc);
            SetBlockInternal(pos, blocks[i]);
        }
    }

    public void SetBlocks(WorldGridPos[] positions, Block[] blocks)
    {
        HashSet<ModuleLocation> moduleLocations = [];
        for (int i = 0; i < positions.Length; i++)
        {
            WorldGridPos pos = positions[i];
            ModuleLocation moduleLoc = pos.ToModuleLocation(data.GridTransform, data.Modules.ModuleSize);
            moduleLocations.Add(moduleLoc);
            SetBlock(pos, blocks[i]);
        }
    }

    public bool TryGetBlock(WorldGridPos worldPos, out Block block)
    {
        ConstructGridPos conPos = worldPos.ToConstruct(data.GridTransform);
        return data.Modules.TryGetBlock(conPos, out block);
    }

    private void SetBlockInternal(ConstructGridPos pos, Block block)
    {
        data.Modules.SetBlock(pos, block);

        if (block.IsEmpty)
        {
            data.Bounds.RemovePosition(pos, data.Modules.Modules);
        }
        else
        {
            data.Bounds.AddPosition(pos);
        }
    }
}
