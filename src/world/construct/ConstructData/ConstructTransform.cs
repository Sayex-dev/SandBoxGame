using Godot;

public partial class ConstructTransform
{
    public WorldGridPos WorldPos;
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
        set
        {
            if (value != Direction.UP && value != Direction.DOWN)
            {
                facingDirection = value;
            }
        }
    }

    public ConstructTransform(WorldGridPos constructWorldPos, Direction FacingDir = Direction.FORWARD)
    {
        WorldPos = constructWorldPos;
        FacingDirection = FacingDir;
    }
}