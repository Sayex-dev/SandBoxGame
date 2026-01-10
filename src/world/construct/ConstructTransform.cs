using Godot;

public class ConstructTransform
{
    public WorldGridPos WorldPos { get; private set; }
    public Vector3I RotationOffset { get; private set; }

    private Direction facingDirection = Direction.FORWARD;
    public Direction FacingDirection
    {
        get { return facingDirection; }
        private set
        {
            if (value != Direction.UP && value != Direction.DOWN)
            {
                facingDirection = value;
            }
        }
    }
    public Vector3I BlockCenter
    {
        get
        {
            if (blockCount == 0)
                return Vector3I.Zero;

            return (Vector3I)((Vector3)positionSum / blockCount).Round();
        }
    }

    private Vector3I positionSum;
    private int blockCount;

    public ConstructTransform(WorldGridPos constructWorldPos, Direction FacingDir = Direction.FORWARD)
    {
        WorldPos = constructWorldPos;
        FacingDirection = FacingDir;
        FacingDir = Direction.FORWARD;
        RotateTo(FacingDir);
    }

    public void MoveTo(WorldGridPos newPos)
    {
        WorldPos = newPos;
    }

    public void RotateTo(Direction newDir)
    {
        FacingDirection = newDir;
    }

    public void RotateLeft()
    {
        RotateTo(DirectionTools.RotateLeft(FacingDirection));
    }

    public void RotateRight()
    {
        RotateTo(DirectionTools.RotateRight(FacingDirection));
    }

    public void AddBlocks(ConstructGridPos pos)
    {
        positionSum += pos.Value;
        blockCount++;
    }

    public void RemoveBlock(ConstructGridPos pos)
    {
        positionSum -= pos.Value;
        blockCount--;
    }
}