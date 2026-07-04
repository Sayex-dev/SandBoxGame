using Godot;

public class ConstructCore
{
    public ConstructData Data { get; }

    public ConstructCore(ConstructData data)
    {
        Data = data;
    }

    public void SetBlock(WorldGridPos pos, Block block)
    {
        ConstructGridPos conPos = pos.ToConstruct(Data.GridTransform);
        SetBlock(conPos, block);
    }

    public void SetBlock(ConstructGridPos pos, Block block)
    {
        SetBlockInternal(pos, block);
    }

    public void RemoveBlock(WorldGridPos pos)
    {
        ConstructGridPos conPos = pos.ToConstruct(Data.GridTransform);
        SetBlock(conPos, new Block());
    }

    public void RemoveBlock(ConstructGridPos pos)
    {
        SetBlock(pos, new Block());
    }

    public void SetBlocks(ConstructGridPos[] positions, Block[] blocks)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            SetBlockInternal(positions[i], blocks[i]);
        }
    }

    public void SetBlocks(WorldGridPos[] positions, Block[] blocks)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            SetBlock(positions[i], blocks[i]);
        }
    }

    public bool TryGetBlock(WorldGridPos worldPos, out Block block)
    {
        ConstructGridPos conPos = worldPos.ToConstruct(Data.GridTransform);
        return TryGetBlock(conPos, out block);
    }

    public bool TryGetBlock(ConstructGridPos conPos, out Block block)
    {
        return Data.Modules.TryGetBlock(conPos, out block);
    }

    private void SetBlockInternal(ConstructGridPos pos, Block block)
    {
        Data.Modules.SetBlock(pos, block);

        if (block.IsEmpty)
        {
            Data.Bounds.RemovePosition(pos);
        }
        else
        {
            Data.Bounds.AddPosition(pos);
        }
    }
}
