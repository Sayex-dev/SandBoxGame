using System;
using Godot;

public readonly struct ConstrucBlockChange
{
    public readonly ConstructGridPos Position;
    public readonly BlockChange Change;

    public ConstrucBlockChange(
        ConstructGridPos pos,
        BlockChangeAction action,
        Block block = default)
    {
        Position = pos;
        Change = new(action, block);
    }
}