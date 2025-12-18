extends Node3D
class_name WorldController

@export var focus_position: Node3D = Node3D.new()
@export var chunk_mat: Material
@export var default_block_store: Array[BlockDefault]

var block_world: BlockWorld
var world_mesh: MeshInstance3D
var chunk_size = Vector3i(16, 16, 16)
var render_distance = Vector3i(5, 2, 5)
var world_clock: WorldClock

var prev_camera_chunk_pos: Vector3i = Vector3i.MAX

func _ready():
	RenderingServer.set_debug_generate_wireframes(true)
	var vp = get_viewport()
	vp.debug_draw = 0
	
	world_clock = WorldClock.new()
	add_child(world_clock)
	
	var world_gen = WorldGenerator.new(chunk_size)
	var ability_manager = AbilityManager.new(world_clock)
	block_world = BlockWorld.new(chunk_size, world_gen, chunk_mat, ability_manager)
	add_child(block_world)
	
	block_world.load_position(focus_position.position, render_distance)

func _physics_process(delta):
	var camera_chunk_pos = Vector3i(focus_position.position) / chunk_size
	if camera_chunk_pos != prev_camera_chunk_pos:
		prev_camera_chunk_pos = camera_chunk_pos
		block_world.load_position(focus_position.position, render_distance)
