public readonly record struct Block
{
    public readonly int Id;
    public readonly Direction Direction;
    public bool IsEmpty => Id == 0;

    public Block(int blockId, Direction direction = Direction.FORWARD)
    {
        Id = blockId;
        Direction = direction;
    }
}