extends Node3D
class_name WorldController

class BlockWorld:
	var loaded_chunks: Dictionary
	
	func add_chunk(chunk_pos: Vector3i, chunk: Chunk, build_mesh: bool=true):
		assert (chunk_pos not in loaded_chunks, "Chunk already exists and cannot be overwritten.")
		loaded_chunks[chunk_pos] = chunk
		if build_mesh:
			build_chunk_mesh(chunk_pos)
	
	func build_chunk_mesh(chunk_pos: Vector3i):
		assert (chunk_pos in loaded_chunks, "Chunk not inside world.")
		var chunk: Chunk = loaded_chunks[chunk_pos]
		var adjacent_chunks = []
		adjacent_chunks.resize(len(chunk.normals))
		for i in len(chunk.normals):
			var normal = chunk.normals[i]
			var adjacent_chunk_pos = chunk_pos + normal
			if adjacent_chunk_pos in loaded_chunks:
				var adjacent_chunk = loaded_chunks[adjacent_chunk_pos]
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
		for chunk_pos in loaded_chunks.keys():
			build_chunk_mesh(chunk_pos)

	func remove_chunk(chunk_index: Vector3i):
		loaded_chunks.erase(chunk_index)

var block_world: BlockWorld
var world_mesh: MeshInstance3D

func _ready():
	RenderingServer.set_debug_generate_wireframes(true)
	var vp = get_viewport()
	vp.debug_draw = 0
	
	var chunk_size = Vector3i(32, 32, 32)
	var world_gen = WorldGenerator.new(chunk_size)
	block_world = BlockWorld.new()
	
	var world_size = Vector3i(10, 2, 10)
	var offset = Vector3i(0, 0, 0)
	var load_positions = []
	for x in range(-int(world_size.x / 2) + offset.x, world_size.x + offset.x):
		for y in range(-int(world_size.y / 2) + offset.y, world_size.y + offset.y):
			for z in range(-int(world_size.z / 2) + offset.z, world_size.z + offset.z):
				load_positions.append(Vector3i(x, y, z))
	
	var chunks: Dictionary
	for chunk_pos in load_positions:
		var chunk = world_gen.generate_chunk(chunk_pos)
		block_world.add_chunk(chunk_pos, chunk, false)
		add_child(chunk)
		chunk.position = chunk_size * chunk_pos
	
	block_world.rebuild_world_mesh()

