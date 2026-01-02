using Godot;
using System;

public interface BlockContainer
{
    public abstract int GetBlock(Vector3I modulePos);
    public abstract bool HasBlock(Vector3I modulePos);
    public abstract void SetBlock(Vector3I modulePos, int blockId);
    public abstract int GetBlockState(Vector3I modulePos);
    public abstract bool HasBlockState(Vector3I modulePos);
    public abstract void SetBlockState(Vector3I modulePos, BlockState blockState);
}
