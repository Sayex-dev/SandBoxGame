extends MeshInstance3D

class_name Chunk 

var chunk_size: Vector3i
var blocks: Array[int]
var block_states: Dictionary # {Vector3i, BlockState}
var chunk_mat: Material

func _init(p_chunk_size: Vector3i, p_chunk_mat: Material) -> void:
	chunk_size = p_chunk_size
	blocks = []
	var block_count = p_chunk_size.x * p_chunk_size.y * p_chunk_size.z
	blocks.resize(block_count)
	blocks.fill(-1)
	chunk_mat = p_chunk_mat

func get_block(chunk_pos: Vector3i) -> int:
	var index: int = chunk_to_array_pos(chunk_pos)
	return blocks[index]

func set_block(chunk_pos: Vector3i, block_id: int):
	var index: int = chunk_to_array_pos(chunk_pos)
	blocks[index] = block_id
	
func set_block_state(chunk_pos: Vector3i, block_state: BlockState):
	block_states[chunk_pos] = block_state

func has_block_state(chunk_pos: Vector3i) -> bool:
	return block_states.has(chunk_pos)

func get_block_state(chunk_pos: Vector3i) -> BlockState:
	return block_states[chunk_pos]

static func world_to_chunk_pos(world_pos: Vector3i, chunk_size: Vector3i, chunk_location: Vector3i) -> Vector3i:
	return world_pos - (chunk_size * chunk_location)

static func wrap_to_chunk(pos: Vector3i, chunk_size: Vector3i) -> Vector3i:
	return Vector3i(
		posmod(pos.x, chunk_size.x),
		posmod(pos.y, chunk_size.y),
		posmod(pos.z, chunk_size.z)
	)

static func chunk_to_world_pos(chunk_location: Vector3i, chunk_size: Vector3i, chunk_pos: Vector3i) -> Vector3i:
	return (chunk_location * chunk_size) + chunk_pos

static func world_to_chunk_location(world_pos: Vector3i, chunk_size: Vector3i) -> Vector3i:
	return Vector3i(
		floori(world_pos.x / chunk_size.x),
		floori(world_pos.y / chunk_size.y),
		floori(world_pos.z / chunk_size.z)
	)

func chunk_to_array_pos(chunk_pos: Vector3i) -> int:
	var index: int = chunk_pos.x + chunk_pos.y * chunk_size.x + chunk_pos.z * chunk_size.x * chunk_size.y
	return index

func array_to_chunk_pos(index: int) -> Vector3i:
	var x = index % chunk_size.x
	@warning_ignore("integer_division")
	var y = int((index / chunk_size.x) % chunk_size.y)
	@warning_ignore("integer_division")
	var z = int(index / (chunk_size.x * chunk_size.y))
	return Vector3i(x, y, z)

func is_in_chunk(chunk_pos: Vector3i) -> bool:
	var correct_x = 0 <= chunk_pos.x and chunk_pos.x < chunk_size.x
	var correct_y = 0 <= chunk_pos.y and chunk_pos.y < chunk_size.y
	var correct_z = 0 <= chunk_pos.z and chunk_pos.z < chunk_size.z
	return correct_x and correct_y and correct_z

func build_mesh(
	x_pos_chunk: Chunk = null,
	x_neg_chunk: Chunk = null,
	y_pos_chunk: Chunk = null,
	y_neg_chunk: Chunk = null,
	z_pos_chunk: Chunk = null,
	z_neg_chunk: Chunk = null):
	
	mesh = ChunkMeshGenerator.build_chunk_mesh(
		self, 
		chunk_mat,
		x_pos_chunk,
		x_neg_chunk,
		y_pos_chunk,
		y_neg_chunk,
		z_pos_chunk,
		z_neg_chunk)
