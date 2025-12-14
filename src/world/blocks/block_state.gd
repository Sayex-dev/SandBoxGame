class_name BlockState

var health_change: int
var additional_active_abilities: Array[ActiveAbility] = []
var remove_active_abilities: Array[int] = []
var ability_cooldown: Dictionary = {}

var additional_passive_abilities: Array[PassiveAbility] = []
var remove_passive_abilities: Array[int] = []

func _init():
	pass
