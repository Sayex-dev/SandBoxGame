using Godot;

public class ActiveState : ISimulationState
{
    private ConstructData data;
    private ConstructPhysicsController physics;
    private ConstructVisualsController visuals;
    private ConstructVisualMotionController visualMotion;

    public ActiveState(
        ConstructData data,
        ConstructPhysicsController physics,
        ConstructVisualsController visuals,
        ConstructVisualMotionController visualMotion)
    {
        this.data = data;
        this.physics = physics;
        this.visuals = visuals;
        this.visualMotion = visualMotion;
    }

    public void OnAddBlock(Block block, ConstructGridPos pos)
    {
        physics.AddBlock(block);
    }

    public void OnEnter(Construct construct)
    {
        throw new System.NotImplementedException();
    }

    public void OnExit()
    {
        throw new System.NotImplementedException();
    }

    public void OnPositionChanged()
    {
        throw new System.NotImplementedException();
    }

    public void Update(double delta)
    {
        if (visualMotion != null)
        {
            visualMotion.Update(delta);
            Position = visualMotion.Position;
            Rotation = visualMotion.Rotation;
        }
        physics.Update(delta);
    }
}