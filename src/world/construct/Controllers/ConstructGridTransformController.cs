using Godot;
using System;

public class ConstructGridTransformController
{
    public event Action<WorldGridPos> OnPositionChanged;
    public event Action<Direction> OnFacingDirectionChanged;

    private WorldGridPos worldPos;
    public WorldGridPos WorldPos
    {
        get => worldPos;
        set
        {
            if (worldPos.Value != value.Value)
            {
                worldPos = value;
                OnPositionChanged?.Invoke(worldPos);
            }
        }
    }
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
                OnFacingDirectionChanged?.Invoke(facingDirection);
            }
        }
    }

    public ConstructGridTransformController(WorldGridPos constructWorldPos, Direction FacingDir = Direction.FORWARD)
    {
        WorldPos = constructWorldPos;
        FacingDirection = FacingDir;
    }
}