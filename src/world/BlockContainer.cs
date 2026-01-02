using Godot;
using System;

public interface BlockContainer
{
    public abstract int GetBlock(Vector3I chunkPos);
    public abstract bool HasBlock(Vector3I chunkPos);
    public abstract void SetBlock(Vector3I chunkPos, int blockId);
    public abstract int GetBlockState(Vector3I chunkPos);
    public abstract bool HasBlockState(Vector3I chunkPos);
    public abstract void SetBlockState(Vector3I chunkPos, BlockState blockState);
}
