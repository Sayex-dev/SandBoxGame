using Godot;

public interface IBlockContainer
{
    public abstract int GetBlock(Vector3I localPos);
    public abstract bool HasBlock(Vector3I localPos);
    public abstract void SetBlock(Vector3I localPos, int blockId);
    public abstract BlockState GetBlockState(Vector3I localPos);
    public abstract bool HasBlockState(Vector3I localPos);
    public abstract void SetBlockState(Vector3I localPos, BlockState blockState);
}
