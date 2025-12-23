using Godot;
using System.Collections.Generic;

public partial class AbilityManager : Node
{
	private WorldClock _worldClock;
	private Dictionary<ActiveAbility.TriggerType, List<Vector3I>> _registeredBlocks
		= new Dictionary<ActiveAbility.TriggerType, List<Vector3I>>();

	public AbilityManager(WorldClock worldClock)
	{
		_worldClock = worldClock;
	}

	public void RegisterBlock(ActiveAbility.TriggerType trigger, Vector3I blockPos)
	{
		if (_registeredBlocks.ContainsKey(trigger))
		{
			_registeredBlocks[trigger].Add(blockPos);
		}
		else
		{
			_registeredBlocks[trigger] = new List<Vector3I> { blockPos };
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// No behavior implemented
	}
}