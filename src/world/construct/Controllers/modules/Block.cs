public readonly record struct Block
{
    public readonly int Id;
    public readonly Direction Direction;
    public readonly Orientation Orientation;
    public bool IsEmpty => Id == 0;

    public Block(int blockId, Direction direction = Direction.FORWARD, Orientation orientation = Orientation.NORTH)
    {
        Id = blockId;
        Direction = direction;
        Orientation = orientation;
    }
}