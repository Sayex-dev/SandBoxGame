extends Node3D

var speed: float = 3
var sprint_speed: float = 10
var rotation_speed: float = 0.0001

func _physics_process(delta):
	var is_sprinting = Input.is_action_pressed("sprint")
	var move_vec = Vector3.ZERO
	
	if Input.is_action_pressed("move_right"):
		# Move as long as the key/button is pressed.
		move_vec += Vector3.RIGHT
	
	if Input.is_action_pressed("move_left"):
		# Move as long as the key/button is pressed.
		move_vec += Vector3.LEFT
	
	if Input.is_action_pressed("move_forward"):
		# Move as long as the key/button is pressed.
		move_vec += Vector3.FORWARD
	
	if Input.is_action_pressed("move_backward"):
		# Move as long as the key/button is pressed.
		move_vec += Vector3.BACK
	 
	if Input.is_action_pressed("move_camera"):
		rotate(Vector3.UP, Input.get_last_mouse_velocity().x * -rotation_speed)
	var move_speed = sprint_speed if is_sprinting else speed
	move_vec = move_vec.normalized() * move_speed * delta 
	position += move_vec.rotated(Vector3.UP, rotation.y)
