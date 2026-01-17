using Godot;

public class ConstructCollision
{
    public ConstructCollider OtherConstruct;
    public Vector3I BlockPosOnOther;
    public Vector3I BlockPosOnThis;
}

public class ConstructCollider
{
    private ExposedSurfaceCache cache;

    public ConstructCollider(ExposedSurfaceCache cache)
    {
        this.cache = cache;
    }

}