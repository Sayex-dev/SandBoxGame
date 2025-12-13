extends Node3D
class_name WorldController

class BlockWorld:
	var chunks: Dictionary
	
	func add_chunk(chunk_pos: Vector3i, chunk: Chunk, build_mesh: bool=true):
		assert (chunk_pos not in chunks, "Chunk already exists and cannot be overwritten.")
		chunks[chunk_pos] = chunk
		if build_mesh:
			build_chunk_mesh(chunk_pos)
	
	func build_chunk_mesh(chunk_pos: Vector3i):
		assert (chunk_pos in chunks, "Chunk not inside world.")
		var chunk: Chunk = chunks[chunk_pos]
		var adjacent_chunks = []
		adjacent_chunks.resize(len(chunk.normals))
		for i in len(chunk.normals):
			var normal = chunk.normals[i]
			var adjacent_chunk_pos = chunk_pos + normal
			if adjacent_chunk_pos in chunks:
				var adjacent_chunk = chunks[adjacent_chunk_pos]
				adjacent_chunks[i] = adjacent_chunk
			
		chunk.build_chunk_mesh(
			adjacent_chunks[0],
			adjacent_chunks[1],
			adjacent_chunks[2],
			adjacent_chunks[3],
			adjacent_chunks[4],
			adjacent_chunks[5]
		)

	func rebuild_world_mesh():
		for chunk_pos in chunks.keys():
			build_chunk_mesh(chunk_pos)

	func remove_chunk(chunk_index: Vector3i):
		var chunk: Chunk = chunks[chunk_index]
		chunks.erase(chunk_index)
		chunk.queue_free()

@export var chunk_mat: Material

var block_world: BlockWorld
var world_mesh: MeshInstance3D
var world_gen: WorldGenerator
var chunk_size = Vector3i(16, 16, 16)
var render_distance = Vector3i(5, 2, 5)
var queued_chunk_pos: Array[Vector3i] = []

@onready var camera: Node3D = $"../Camera3D"
var prev_camera_chunk_pos: Vector3i = Vector3i.MAX


func _ready():
	RenderingServer.set_debug_generate_wireframes(true)
	var vp = get_viewport()
	vp.debug_draw = 0
	
	world_gen = WorldGenerator.new(chunk_size)
	block_world = BlockWorld.new()
	
	load_position(camera.position)

func _physics_process(delta):
	var camera_chunk_pos = Vector3i(camera.position) / chunk_size
	if camera_chunk_pos != prev_camera_chunk_pos:
		prev_camera_chunk_pos = camera_chunk_pos
		load_position(camera.position)

func load_position(world_pos: Vector3):
	var load_chunk_pos = Vector3i((world_pos / Vector3(chunk_size)).floor())
	
	var desired_chunks: Array[Vector3i] = []
	var add_chunks: Array[Vector3i] = []
	var remove_chunks: Array[Vector3i] = []
	
	for x in range(-render_distance.x, render_distance.x):
		for y in range(-render_distance.y, render_distance.y):
			for z in range(-render_distance.z, render_distance.z):
				desired_chunks.append(load_chunk_pos + Vector3i(x, y, z))

	for chunk_pos in desired_chunks:
		if not block_world.chunks.has(chunk_pos) and chunk_pos not in queued_chunk_pos:
			queued_chunk_pos.append(chunk_pos)
			add_chunks.append(chunk_pos)

	for chunk_pos in block_world.chunks.keys():
		if not desired_chunks.has(chunk_pos):
			remove_chunks.append(chunk_pos)
		if chunk_pos in queued_chunk_pos:
			queued_chunk_pos.erase(chunk_pos)

	update_chunk_loading(add_chunks, remove_chunks)

func update_chunk_loading(load_positions: Array[Vector3i] = [], unload_positions: Array[Vector3i] = []):
	# unload first
	for chunk_pos in unload_positions:
		if block_world.chunks.has(chunk_pos):
			var chunk = block_world.chunks[chunk_pos]
			block_world.remove_chunk(chunk_pos)
			chunk.queue_free()

	# load chunks
	var thread = Thread.new()
	thread.start(_generate_chunks.bind(load_positions, thread))

func gather_chunks(thread: Thread):
	var chunks: Dictionary = thread.wait_to_finish()
	for chunk_pos in chunks.keys():
		var chunk = chunks[chunk_pos]
		if chunk_pos in queued_chunk_pos:
			queued_chunk_pos.erase(chunk_pos)
			block_world.add_chunk(chunk_pos, chunk, false)
			add_child(chunk)
		else:
			chunk.queue_free()

func _generate_chunks(load_positions: Array[Vector3i], thread: Thread):
	var chunks: Dictionary = {}
	for chunk_pos in load_positions:
		var chunk = world_gen.generate_chunk(chunk_pos, chunk_mat)
		chunk.build_chunk_mesh()
		chunk.position = chunk_size * chunk_pos
		chunks[chunk_pos] = chunk
	call_deferred("gather_chunks", thread)
	return chunks
