class_name WorldGenerator

var chunk_size: Vector3i
var noise: FastNoiseLite

func _init(_chunk_size: Vector3i):
	chunk_size = _chunk_size
	noise = FastNoiseLite.new()
	noise.noise_type = FastNoiseLite.TYPE_SIMPLEX
	
func generate_chunk(chunk_pos: Vector3i, chunk_mat: Material) -> Chunk:
	var chunk = Chunk.new(chunk_size, chunk_mat)
	_set_ground(chunk, chunk_pos)
	return chunk

func _set_ground(chunk: Chunk, chunk_pos: Vector3i):
	for x in range(chunk_size.x):
		for z in range(chunk_size.z):
			var x_pos = chunk_pos.x * chunk_size.x + x
			var z_pos = chunk_pos.z * chunk_size.z + z
			var noise_height = int(noise.get_noise_2d(x_pos, z_pos) * 10)
			for y in range(min(noise_height - chunk_pos.y * chunk_size.y, chunk_size.y)):
				var in_chunk_pos = Vector3i(x, y, z)
				chunk.set_block(in_chunk_pos, 1)
