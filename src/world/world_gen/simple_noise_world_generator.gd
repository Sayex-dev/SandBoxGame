extends WorldGenerator
class_name SimpleNoiseWorldGenerator

@export var gen_offset: Vector3i

var noise: FastNoiseLite = FastNoiseLite.new()

func _init():
	noise.noise_type = FastNoiseLite.TYPE_SIMPLEX
	
func generate_chunk(
	chunk_location: Vector3i, 
	chunk_mat: Material, 
	chunk_size: Vector3i) -> Chunk:
	var chunk = Chunk.new(chunk_size, chunk_mat)
	_set_ground(chunk, chunk_location)
	return chunk

func _set_ground(chunk: Chunk, chunk_location: Vector3i):
	var chunk_size = chunk.chunk_size
	for x in range(chunk_size.x):
		for z in range(chunk_size.z):
			var x_pos = chunk_location.x * chunk_size.x + x + gen_offset.x
			var z_pos = chunk_location.z * chunk_size.z + z + gen_offset.z
			var noise_height = int(noise.get_noise_2d(x_pos, z_pos) * 10) + gen_offset.y
			for y in range(min(noise_height - chunk_location.y * chunk_size.y, chunk_size.y)):
				var in_chunk_pos = Vector3i(x, y, z)
				chunk.set_block(in_chunk_pos, 1)
