using Godot;
using Godot.Collections;

public partial class BlockState : Node
{
	public int HealthChange { get; set; }
	public float WeightChange { get; set; }

	public Array<ActiveAbility> AdditionalActiveAbilities { get; set; } = new();
	public Array<int> RemoveActiveAbilities { get; set; } = new();
	public Dictionary AbilityCooldown { get; set; } = new();

	public Array<PassiveAbility> AdditionalPassiveAbilities { get; set; } = new();
	public Array<int> RemovePassiveAbilities { get; set; } = new();
}