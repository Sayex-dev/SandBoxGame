extends Resource
class_name ActiveAbility

enum TriggerType {
	RANDOM_TICK,
	ON_UPDATED,
	PLAYER_FORWARD,
	PLAYER_BACKWARD,
	PLAYER_LEFT,
	PLAYER_RIGHT
}

@export var trigger: TriggerType
@export var refire_cooldown: int

func trigger_ability():
	assert(false, "ActiveAbility base class cannot be executed.")
