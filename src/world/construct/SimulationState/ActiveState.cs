public class ActiveState : ISimulationState
{
    private ConstructData data;
    private ConstructPhysicsController physics;
    private ConstructVisualsController visuals;
    private ConstructVisualMotionController motion;

    public ActiveState(
        ConstructData data,
        ConstructPhysicsController physics, 
        ConstructVisualsController visuals,
        ConstructVisualMotionController motion)
    {
        this.data = data;
        this.physics = physics;
        this.visuals = visuals;
        this.motion = motion;
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
		throw new System.NotImplementedException();
	}
}