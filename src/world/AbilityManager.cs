using Godot;
using System.Collections.Generic;

public partial class AbilityManager : Node
{
	private WorldClock worldClock;
	private Dictionary<ActiveAbility.TriggerType, List<Vector3I>> registeredBlocks
		= new Dictionary<ActiveAbility.TriggerType, List<Vector3I>>();

	public AbilityManager(WorldClock worldClock)
	{
		this.worldClock = worldClock;
	}

	public void RegisterBlock(ActiveAbility.TriggerType trigger, Vector3I blockPos)
	{
		if (registeredBlocks.ContainsKey(trigger))
		{
			registeredBlocks[trigger].Add(blockPos);
		}
		else
		{
			registeredBlocks[trigger] = new List<Vector3I> { blockPos };
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// No behavior implemented
	}
}