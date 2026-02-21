using Godot;

public class ConstructMotionController
{
    private readonly ConstructData data;
    private readonly IWorldCollisionQuery collisionQuery;

    public ConstructMotionController(ConstructData data, IWorldCollisionQuery collisionQuery)
    {
        this.data = data;
        this.collisionQuery = collisionQuery;
    }

    public void RotateTo(Direction newDir, WorldGridPos rotationCenter)
    {
        if (newDir == data.Transform.FacingDirection)
            return;

        Vector3 oldFacing = DirectionTools.GetWorldDirVec(data.Transform.FacingDirection);
        Vector3 newFacing = DirectionTools.GetWorldDirVec(newDir);

        float deltaAngle = oldFacing.SignedAngleTo(newFacing, Vector3.Up);
        data.Transform.WorldPos = new(rotationCenter.Value + (Vector3I)((Vector3)(data.Transform.WorldPos.Value - rotationCenter.Value)).Rotated(Vector3.Up, deltaAngle));
        data.Transform.FacingDirection = newDir;
    }

    public void RotateLeft(WorldGridPos rotationCenter)
    {
        RotateTo(DirectionTools.RotateLeft(data.Transform.FacingDirection), rotationCenter);
    }

    public void RotateRight(WorldGridPos rotationCenter)
    {
        RotateTo(DirectionTools.RotateRight(data.Transform.FacingDirection), rotationCenter);
    }

    public bool TryMoveTo(WorldGridPos newPos)
    {
        Vector3I div = data.Transform.WorldPos.Value - newPos.Value;
        return TryMoveBy(div);
    }

    public bool TryMoveBy(Vector3I div)
    {
        bool isStep = div.Length() == 1;
        if (isStep)
        {
            Direction dir = DirectionTools.GetClosestDirection(div);
            return TryTakeStep(dir);
        }
        return false;
    }

    public bool TryTakeStep(Direction dir)
    {
        if (CanStepIntoDir(dir))
        {
            data.Transform.WorldPos += (Vector3I)DirectionTools.GetWorldDirVec(dir);
            return true;
        }
        return false;
    }

    private bool CanStepIntoDir(Direction dir)
    {
        Vector3I step = (Vector3I)DirectionTools.GetWorldDirVec(dir);
        WorldGridPos targetMin = new(data.Bounds.MinPos.ToWorld(data.Transform).Value + step);
        WorldGridPos targetMax = new(data.Bounds.MaxPos.ToWorld(data.Transform).Value + step);

        var nearConstructs = collisionQuery.GetConstructsInArea(targetMin, targetMax);
        foreach (var other in nearConstructs)
        {
            if (other.Data == data)
                continue;

            foreach (var (moduleLocation, module) in data.Modules.Modules)
            {
                foreach (var facePos in module.SurfaceCache.ExposedSurfaces[dir])
                {
                    var faceWorldPos = facePos.ToWorld(moduleLocation, data.Transform, data.Modules.ModuleSize);
                    if (other.TryGetBlock(faceWorldPos + step, out _))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
}