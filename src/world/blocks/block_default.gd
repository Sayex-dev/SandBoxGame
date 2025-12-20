extends Resource
class_name BlockDefault

@export var name: String
@export var health: int
@export var passive_abilities: Array[PassiveAbility]
@export var active_abilities: Array[ActiveAbility]

@export var texture_atlas_face_up: Vector2i
@export var texture_atlas_face_down: Vector2i
@export var texture_atlas_face_left: Vector2i
@export var texture_atlas_face_right: Vector2i
@export var texture_atlas_face_forwad: Vector2i
@export var texture_atlas_face_backward: Vector2i
