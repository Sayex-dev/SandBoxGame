using Godot;

public class ConstructGridTransform
{
    public WorldGridPos WorldPos { get; private set; }
    public float YRotation
    {
        get
        {
            Vector3 facingVec = DirectionTools.GetWorldDirVec(FacingDirection);
            float angleToForward = Mathf.PosMod(Vector3.Forward.SignedAngleTo(facingVec, Vector3.Up), 2 * Mathf.Pi);
            return Mathf.RadToDeg(angleToForward);
        }
    }
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
    public ConstructGridPos BlockCenter
    {
        get
        {
            if (blockCount == 0)
                return new(Vector3I.Zero);

            return new((Vector3I)((Vector3)positionSum / blockCount).Round());
        }
    }

    private Vector3I positionSum;
    private int blockCount;

    public ConstructGridTransform(WorldGridPos constructWorldPos, Direction FacingDir = Direction.FORWARD)
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
        if (newDir == FacingDirection) return;

        Vector3 oldFacing = DirectionTools.GetWorldDirVec(FacingDirection);
        Vector3 newFacing = DirectionTools.GetWorldDirVec(newDir);

        float deltaAngle = oldFacing.SignedAngleTo(newFacing, Vector3.Up);
        WorldPos = new(WorldPos.Value + BlockCenter.Value - (Vector3I)((Vector3)BlockCenter.Value).Rotated(Vector3.Up, deltaAngle));
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