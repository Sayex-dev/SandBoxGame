public class ConstructSimulationController
{
	public ConstructVisualMotionController visualMotion;

	public ConstructSimulationController(ConstructVisualMotionController visualMotionController)
	{
		visualMotion = visualMotionController;
	}

    public void Update(double delta)
    {
        if (SimulationState == SimulationState.ACTIVE)
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
}