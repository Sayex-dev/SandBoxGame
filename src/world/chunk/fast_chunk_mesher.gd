class_name FastChunkMesher

static func build_chunk_mesh(chunk: Chunk, chunk_mat: Material):
	if max(chunk.chunk_size) > 63:
		assert(false, "Chunk size cannot be bigger than 63")
	var voxel_grid = prepare_voxel_grid(chunk.blocks)

static func prepare_voxel_grid(blocks: Array[int]):
	var voxel_grid: Dictionary = [] # {block_id: PackedInt64Array
	PackedInt64Array
	for block_i in range(blocks.size()):
		pass
	pass
