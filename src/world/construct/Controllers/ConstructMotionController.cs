using Godot;

public class ConstructMotionController
{
    private readonly ConstructData data;
    private readonly IWorldQuery collisionQuery;

    public ConstructMotionController(ConstructData data, IWorldQuery collisionQuery)
    {
        this.data = data;
        this.collisionQuery = collisionQuery;
    }

    public void RotateTo(Direction newDir, WorldGridPos rotationCenter)
    {
        if (newDir == data.GridTransform.FacingDirection)
            return;

        Vector3 oldFacing = DirectionTools.GetWorldDirVec(data.GridTransform.FacingDirection);
        Vector3 newFacing = DirectionTools.GetWorldDirVec(newDir);

        float deltaAngle = oldFacing.SignedAngleTo(newFacing, Vector3.Up);
        data.GridTransform.WorldPos = new(rotationCenter.Value + (Vector3I)((Vector3)(data.GridTransform.WorldPos.Value - rotationCenter.Value)).Rotated(Vector3.Up, deltaAngle));
        data.GridTransform.FacingDirection = newDir;
    }

    public void RotateLeft(WorldGridPos rotationCenter)
    {
        RotateTo(DirectionTools.RotateLeft(data.GridTransform.FacingDirection), rotationCenter);
    }

    public void RotateRight(WorldGridPos rotationCenter)
    {
        RotateTo(DirectionTools.RotateRight(data.GridTransform.FacingDirection), rotationCenter);
    }

    public bool TryMoveTo(WorldGridPos newPos)
    {
        Vector3I div = data.GridTransform.WorldPos.Value - newPos.Value;
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
        TimeTracker.Start("Take step time", TimeTracker.TrackingType.Average);
        if (CanStepIntoDir(dir))
        {
            data.GridTransform.WorldPos += (Vector3I)DirectionTools.GetWorldDirVec(dir);
            return true;
        }
        TimeTracker.End("Take step time");
        return false;
    }

    private bool CanStepIntoDir(Direction dir)
    {
        Vector3I step = (Vector3I)DirectionTools.GetWorldDirVec(dir);
        WorldGridPos targetMin = new(data.Bounds.MinPos.ToWorld(data.GridTransform).Value + step);
        WorldGridPos targetMax = new(data.Bounds.MaxPos.ToWorld(data.GridTransform).Value + step);

        var nearConstructs = collisionQuery.GetConstructsInArea(targetMin, targetMax);
        foreach (var other in nearConstructs)
        {
            if (other.Core.Data == data)
                continue;

            foreach (var (moduleLocation, module) in data.Modules.Modules)
            {
                foreach (var facePos in module.SurfaceCache.CollisionCache.GetAllExposedSurfaces()[dir])
                {
                    var faceWorldPos = facePos.ToWorld(moduleLocation, data.GridTransform, data.Modules.ModuleSize);
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