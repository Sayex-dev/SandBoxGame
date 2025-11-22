class_name WorldGenerator

var chunk_size: Vector3i

func _init(_chunk_size: Vector3i):
	chunk_size = _chunk_size

func generate_chunk(chunk_pos: Vector3i) -> Chunk:
	var chunk = Chunk.new(chunk_size)
	return chunk
