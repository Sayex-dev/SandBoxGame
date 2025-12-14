extends Node
class_name AbilityManager

var world_clock: WorldClock
var registered_blocks: Dictionary = {} # {Trigger: Array[block_pos]}

func _init(p_world_clock: WorldClock):
	world_clock = p_world_clock

func register_block(trigger: ActiveAbility.TriggerType, block_pos: Vector3i):
	if registered_blocks.has(trigger):
		registered_blocks[trigger].append(block_pos)
	else:
		registered_blocks[trigger] = [block_pos]

func _physics_process(delta):
	pass
