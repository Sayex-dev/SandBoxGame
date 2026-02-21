using System;
using Godot;

public enum BlockChangeAction
{
    KEEP_PREVIOUS,
    REPLACE,
    REMOVE
}

public readonly struct BlockChange
{
    public readonly BlockChangeAction Action;
    public readonly Block Block;

    // Default constructor with optional parameters
    public BlockChange(
        BlockChangeAction action = BlockChangeAction.KEEP_PREVIOUS,
        Block block = default)
    {
        Action = action;
        if (action == BlockChangeAction.REPLACE && block.IsEmpty)
        {
            throw new Exception("Block parameter cannot be null if action replace is selected.");
        }

        Block = block;
    }
}