using Godot;

public class ApproximatedState : SimulationState
{
	public ApproximatedState(ConstructCore core) : base(core) { }

	public override Vector3 GetPosition()
	{
		throw new System.NotImplementedException();
	}

	public override Vector3 GetRotation()
	{
		return Vector3.Zero;
	}
}