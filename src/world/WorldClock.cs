using Godot;
using System.Collections.Generic;

public partial class WorldClock : Node
{
	public int WorldTick { get; private set; }
	private Dictionary<int, List<Callable>> _events = new();

	public WorldClock(int worldTick = 0)
	{
		WorldTick = worldTick;
	}

	public override void _PhysicsProcess(double delta)
	{
		WorldTick += 1;

		if (_events.TryGetValue(WorldTick, out List<Callable> eventList))
		{
			foreach (var ev in eventList)
			{
				ev.Call();
			}
		}
	}

	public void RegisterEvent(int atTick, Callable function)
	{
		if (atTick > WorldTick)
		{
			if (!_events.ContainsKey(atTick))
			{
				_events[atTick] = new List<Callable>();
			}

			_events[atTick].Add(function);
		}
		else
		{
			GD.PushError(
				$"Event at game tick {atTick} has been set after world tick time {WorldTick}. Event will be ignored."
			);
		}
	}
}