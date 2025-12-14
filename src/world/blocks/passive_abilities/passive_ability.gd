extends Resource
class_name PassiveAbility

enum PassiveAbilityType {
	ARMOR
}

@export var level: int
@export var type: PassiveAbilityType

# Make sure that every parameter has a default value.
# Otherwise, there will be problems with creating and editing
# your resource via the inspector.
func _init(p_level = 1, p_type = PassiveAbilityType.ARMOR):
	level = p_level
	type = p_type
