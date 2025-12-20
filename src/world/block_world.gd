extends Node3D
class_name BlockWorld
var chunks: Dictionary
var queued_chunk_pos: Array[Vector3i] = []

var world_gen: WorldGenerator
var chunk_size: Vector3i
var chunk_mat: Material
var ability_manager: AbilityManager

func _init(
	p_chunk_size: Vector3i, 
	p_world_gen: WorldGenerator,
	p_chunk_mat: Material, 
	p_ability_manager: AbilityManager
):
	chunk_size = p_chunk_size
	world_gen = p_world_gen
	chunk_mat = p_chunk_mat
	ability_manager = p_ability_manager

func set_block_state(world_pos: Vector3i, block_state: BlockState):
	var chunk_loc = Chunk.world_to_chunk_location(world_pos, chunk_size)
	var chunk_pos = Chunk.wrap_to_chunk(world_pos, chunk_size)
	chunks[chunk_loc].set_block_state(chunk_pos, block_state)

func get_block_state(world_pos: Vector3i) -> BlockState:
	var chunk_loc = Chunk.world_to_chunk_location(world_pos, chunk_size)
	var chunk_pos = Chunk.wrap_to_chunk(world_pos, chunk_size)
	return chunks[chunk_loc]

func has_block_state(world_pos: Vector3i) -> bool:
	var chunk_loc = Chunk.world_to_chunk_location(world_pos, chunk_size)
	var chunk_pos = Chunk.wrap_to_chunk(world_pos, chunk_size)
	return chunks[chunk_loc].has_block_state(chunk_pos)

func load_position(world_pos: Vector3, render_distance: Vector3i):
	var load_chunk_pos = Vector3i((world_pos / Vector3(chunk_size)).floor())
	
	var desired_chunks: Array[Vector3i] = []
	var add_chunks: Array[Vector3i] = []
	var remove_chunks: Array[Vector3i] = []
	
	for x in range(-render_distance.x, render_distance.x):
		for y in range(-render_distance.y, render_distance.y):
			for z in range(-render_distance.z, render_distance.z):
				desired_chunks.append(load_chunk_pos + Vector3i(x, y, z))

	for chunk_pos in desired_chunks:
		if not chunks.has(chunk_pos) and chunk_pos not in queued_chunk_pos:
			queued_chunk_pos.append(chunk_pos)
			add_chunks.append(chunk_pos)

	for chunk_pos in chunks.keys():
		if not desired_chunks.has(chunk_pos):
			remove_chunks.append(chunk_pos)
		if chunk_pos in queued_chunk_pos:
			queued_chunk_pos.erase(chunk_pos)

	update_chunk_loading(add_chunks, remove_chunks)

func update_chunk_loading(load_positions: Array[Vector3i] = [], unload_positions: Array[Vector3i] = []):
	# unload first
	for chunk_pos in unload_positions:
		if chunks.has(chunk_pos):
			var chunk = chunks[chunk_pos]
			chunks.erase(chunk_pos)
			chunk.queue_free()

	# load chunks
	var thread = Thread.new()
	thread.start(_generate_chunks.bind(load_positions, thread))

func _generate_chunks(load_positions: Array[Vector3i], thread: Thread):
	var chunks: Dictionary = {}
	for chunk_pos in load_positions:
		var chunk = world_gen.generate_chunk(chunk_pos, chunk_mat, chunk_size)
		chunk.build_mesh()
		chunk.position = chunk_size * chunk_pos
		chunks[chunk_pos] = chunk
	call_deferred("gather_chunks", thread)
	return chunks

func gather_chunks(thread: Thread):
	var chunks: Dictionary = thread.wait_to_finish()
	for chunk_pos in chunks.keys():
		var chunk = chunks[chunk_pos]
		if chunk_pos in queued_chunk_pos:
			queued_chunk_pos.erase(chunk_pos)
			chunks[chunk_pos] = chunk
			add_child(chunk)
		else:
			chunk.queue_free()
