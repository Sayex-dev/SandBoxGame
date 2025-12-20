extends Resource
class_name WorldGenerator

func generate_chunk(
	chunk_location: Vector3i, 
	chunk_mat: Material, 
	chunk_size: Vector3i) -> Chunk:
	assert(false)
	return Chunk.new(chunk_size, chunk_mat)
