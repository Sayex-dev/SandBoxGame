extends WorldGenerator
class_name PresetWorldGenerator

@export var blocks: Array[Vector4i]
@export var offset: Vector3i

func generate_chunk(
	chunk_location: Vector3i, 
	chunk_mat: Material, 
	chunk_size: Vector3i) -> Chunk:
	var chunk = Chunk.new(chunk_size, chunk_mat)
	for block in blocks:
		var world_pos = Vector3i(block.x, block.y, block.z) + offset
		var in_chunk_pos = Chunk.world_to_chunk_pos(world_pos, chunk_size, chunk_location)
		if chunk.is_in_chunk(in_chunk_pos):
			chunk.set_block(in_chunk_pos, block.w)
	return chunk
