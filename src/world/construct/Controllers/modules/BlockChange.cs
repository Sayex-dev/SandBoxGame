using System;
using Godot;

public enum BlockChangeAction
{
    PLACE,
    REMOVE
}

public readonly struct BlockChange<T> where T : struct, IGridPos
{
    public readonly T Position;
    public readonly BlockChangeAction Action;
    public readonly Block Block;

    // Default constructor with optional parameters
    public BlockChange(
        T pos,
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