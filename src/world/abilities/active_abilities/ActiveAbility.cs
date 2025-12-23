using Godot;

[GlobalClass]
public partial class ActiveAbility : Resource
{
	public enum TriggerType
	{
		RandomTick,
		OnUpdated,
		PlayerForward,
		PlayerBackward,
		PlayerLeft,
		PlayerRight
	}

	[Export]
	public TriggerType Trigger { get; set; }

	[Export]
	public int RefireCooldown { get; set; }

	public virtual void TriggerAbility()
	{
		throw new System.Exception("ActiveAbility base class cannot be executed.");
	}
}