using System;
using Godot;

public enum BlockChangeAction
{
    PLACE,
    REMOVE
}

public readonly struct BlockChange
{
    public readonly ModuleGridPos Position;
    public readonly BlockChangeAction Action;
    public readonly Block Block;

    // Default constructor with optional parameters
    public BlockChange(
        ModuleGridPos pos,
        BlockChangeAction action,
        Block block = default)
    {
        Action = action;
        if (action == BlockChangeAction.PLACE && block.IsEmpty)
        {
            throw new Exception("Block parameter cannot be null if action replace is selected.");
        }

        Position = pos;
        Block = block;
    }
}