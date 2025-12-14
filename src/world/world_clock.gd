extends Node
class_name WorldClock

var world_tick: int
var events: Dictionary = {}

func _init(p_world_tick: int=0):
	world_tick = p_world_tick

func _physics_process(delta):
	world_tick += 1
	for event in events.get(world_tick, []):
		event.call()

func register_event(at_tick: int, function: Callable):
	if at_tick > world_tick:
		if events.has(at_tick):
			events[at_tick].append(function)
		else:
			events[at_tick] = [function]
	else:
		push_error("Event at game tick " + str(at_tick) + " has been set after world tick time " + str(world_tick) + ". Event will be ignored.")
