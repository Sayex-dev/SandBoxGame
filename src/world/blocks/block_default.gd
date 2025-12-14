extends Resource
class_name BlockDefault

@export var block_id: int
@export var health: int
@export var passive_abilities: Array[PassiveAbility]
@export var active_abilities: Array[ActiveAbility]

# Make sure that every parameter has a default value.
# Otherwise, there will be problems with creating and editing
# your resource via the inspector.
func _init(p_block_id = 0, p_health = 0, p_active_abilities = {}):
	health = p_health
	block_id = p_block_id
	active_abilities = p_active_abilities
