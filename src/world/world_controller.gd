extends Node3D
class_name WorldController

class BlockWorld:
	var loaded_chunks: Dictionary
	
	func add_chunk(chunk_index: Vector3i, chunk: Chunk):
		assert (chunk_index not in loaded_chunks, "Chunk already exists and cannot be overwritten.")
		loaded_chunks[chunk_index] = chunk
		chunk.build_chunk_mesh()

	func remove_chunk(chunk_index: Vector3i):
		loaded_chunks.erase(chunk_index)

var block_world: BlockWorld
var world_mesh: MeshInstance3D

func _ready():
	var chunk_size = Vector3i(4, 4, 4)
	var world_gen = WorldGenerator.new(chunk_size)
	#var chunk = world_gen.generate_chunk(Vector3i.ZERO)
	var chunk = Chunk.new(chunk_size)
	chunk.set_block(Vector3i.ZERO, 1)
	block_world = BlockWorld.new()
	block_world.add_chunk(Vector3i.ZERO, chunk)
	add_child(chunk)
