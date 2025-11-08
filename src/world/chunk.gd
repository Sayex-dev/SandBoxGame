extends Node

class_name Chunk

var normals = [
	Vector3i(1, 0, 0), 
	Vector3i(-1, 0, 0), 
	Vector3i(0, 1, 0), 
	Vector3i(0, -1, 0),
	Vector3i(0, 0, 1), 
	Vector3i(0, 0, -1)
]

class Surface:
	var vertices: Array[Vector3] = []
	var uvs: Array[Vector2] = []
	
	func _init(_vertices: PackedVector3Array, _uvs: PackedVector2Array):
		vertices = _vertices
		uvs = _uvs

var _mesh: MeshInstance3D
var _chunk_size: Vector3i
var _blocks: Array[int]

func _init(chunk_size: Vector3i) -> void:
	_chunk_size = chunk_size
	_blocks = []
	var block_count = _chunk_size.x * _chunk_size.y * _chunk_size.z
	_blocks.resize(block_count)
	_blocks.fill(-1)

func get_block(chunk_pos: Vector3i) -> int:
	var index: int = chunk_to_array_pos(chunk_pos)
	assert (not is_in_chunk(index), "Pos not in chunk: " + str(chunk_pos))
		
	return _blocks[index]

func set_block(chunk_pos: Vector3i, block_id: int):
	var index: int = chunk_to_array_pos(chunk_pos)
	assert (not is_in_chunk(index), "Pos not in chunk: " + str(chunk_pos))
	_blocks[index] = block_id
	
func chunk_to_array_pos(chunk_pos: Vector3i) -> int:
	var index: int = chunk_pos.x + chunk_pos.y * _chunk_size.x + chunk_pos.z * _chunk_size.x * _chunk_size.y
	return index

func array_to_chunk_pos(index: int) -> Vector3i:
	var x = index % _chunk_size.x
	@warning_ignore("integer_division")
	var y = int(index / _chunk_size.x) % _chunk_size.y
	@warning_ignore("integer_division")
	var z = int(index / _chunk_size.x) * _chunk_size.y
	return Vector3i(x, y, z)

func is_in_chunk(index: int) -> bool:
	return 0 >= index and index < (_chunk_size.x * _chunk_size.y * _chunk_size.z)

func find_block_surfaces(
	x_pos_chunk: Chunk,
	x_neg_chunk: Chunk,
	y_pos_chunk: Chunk,
	y_neg_chunk: Chunk,
	z_pos_chunk: Chunk,
	z_neg_chunk: Chunk
) -> Dictionary:
	
	# Find all exposed block surfaces
	var exposed_blocks: Dictionary
	var adjacent_chunks = [
		x_pos_chunk,
		x_neg_chunk,
		y_pos_chunk,
		y_neg_chunk,
		z_pos_chunk,
		z_neg_chunk
	]
	for z in range(_chunk_size.z):
		for y in range(_chunk_size.y):
			for x in range(_chunk_size.x):
				var block_pos = Vector3i(x, y, z)
				
				# Skip Air Blocks
				if get_block(block_pos) == -1:
					continue
				
				# Find exposed block surfaces
				var exposed_surfaces = []
				exposed_surfaces.resize(len(normals))
				var has_exposed_surfaces = false
				for i in len(normals):
					var dir = normals[i]
					var adjacent_pos = block_pos + dir
					var adjacent_block
					if is_in_chunk(chunk_to_array_pos(adjacent_pos)):
						adjacent_block = get_block(adjacent_pos)
					else:
						var adjacent_chunk_pos = adjacent_pos % _chunk_size
						adjacent_block = adjacent_chunks[i].get_block(adjacent_chunk_pos)
					if adjacent_block != -1:
						has_exposed_surfaces = true
						exposed_surfaces[i] = true
				if has_exposed_surfaces:
					exposed_blocks[block_pos] = exposed_surfaces
	return exposed_blocks

func get_surface_vectors(exposed_block_surfaces: Dictionary) -> Array[Surface]:
	var block_surfaces = {}
	for pos in exposed_block_surfaces:
		block_surfaces[pos] = exposed_block_surfaces[pos].duplicate()
	
	var max_dim: int = max(_chunk_size.x, _chunk_size.y, _chunk_size.z)
	var surface_vector_dict: Array[Surface] = []
	while len(block_surfaces) > 0:
		# Find a surface on a block
		# Example:
		# [ ][ ][ ]
		#    [X][ ]
		# [ ][ ][ ]
		var start_pos: Vector3i = block_surfaces.keys()[0]
		var dir_i: int
		var normal: Vector3i
		for normal_i in range(6):
			if block_surfaces[start_pos][normal_i]:
				block_surfaces[start_pos][normal_i] = false
				normal = normals[normal_i]
				dir_i = normal_i
				break
		
		# Compute the step directions depending on the view of the surface
		var loc_x_move: Vector3i = Vector3i(embed_2d_in_plane(Vector2(1, 0), normal))
		var loc_y_move: Vector3i = Vector3i(embed_2d_in_plane(Vector2(0, 1), normal))
		
		# Find min pos by alternating between moving in -x and -y directions
		# until both steps fail.
		# Example:
		# [ ][ ][ ]
		#    [ ][ ]
		# [X][ ][ ]
		var min_pos: Vector3i = start_pos
		var move_x: bool = true
		var failed_last: bool = false
		for i in range(max_dim * max_dim):
			var new_pos: Vector3i
			if move_x:
				new_pos = min_pos - loc_x_move
			else:
				new_pos = min_pos - loc_y_move
			var has_surface = new_pos in block_surfaces and block_surfaces[new_pos][dir_i]
			if has_surface:
				min_pos = new_pos
				failed_last = false
			elif not failed_last:
				failed_last = true
				move_x = not move_x
			else:
				# Found min pos
				break
		
		# Greedy iterate over the square surface to find max_pos
		# Example:
		# [ ][ ][ ]
		#    [ ][ ]
		# [X][X][X]
		var max_x: int = -1
		var max_pos: Vector3i
		for y in range(max_dim + 1):
			var has_full_row: bool = false
			for x in range(max_dim + 1):
				var new_pos = min_pos + loc_x_move * x + loc_y_move * y
				var has_surface = new_pos in block_surfaces and block_surfaces[new_pos][dir_i]
				var is_first_row = max_x == -1
				var is_last_in_rectangle = x == max_x
				# Skip surfaces that are ok
				if has_surface and not is_last_in_rectangle:
					continue
				# End first row case overstepped surface in x
				elif not has_surface and is_first_row:
					max_x = x - 1
					has_full_row = true
					max_pos = new_pos - loc_x_move
					break
				# End row in square if x is at x_max
				elif has_surface and is_last_in_rectangle:
					has_full_row = true
					max_pos = new_pos
					break
				# End row with not enough squares
				else:
					has_full_row = false
					break
			
			# Terminate if row is not extending rectangle
			if not has_full_row:
				break
		
		# Remove surfaces that have been used up
		for z in range(min_pos.z, max_pos.z + 1):
			for y in range(min_pos.y, max_pos.y + 1):
				for x in range(min_pos.x, max_pos.x + 1):
					var remove_pos = Vector3i(x, y, z)
					block_surfaces[remove_pos][dir_i] = false
		
		# Find corner vectors of surface
		# This vector converts the block coordinate min pos to world coordinate of the surface
		var min_conversion_vectors = [
			Vector3i(1, 0, 0),
			Vector3i(0, 0, 0),
			Vector3i(0, 1, 0),
			Vector3i(0, 0, 0),
			Vector3i(0, 0, 1),
			Vector3i(0, 0, 0),
		] 
		# This vector converts the block coordinate max pos to world coordinate of the surface
		var max_conversion_vectors = [
			Vector3i(1, 1, 1),
			Vector3i(0, 1, 1),
			Vector3i(1, 1, 1),
			Vector3i(1, 0, 1),
			Vector3i(1, 1, 1),
			Vector3i(1, 1, 0),
		] 
		var min_world_pos = min_pos + min_conversion_vectors[dir_i]
		var max_world_pos = max_pos + min_conversion_vectors[dir_i]
		
		# Find the corner vertices
		# TODO: Find the corner vertices
		
		# Compute vertices
		var vertices: PackedVector3Array = PackedVector3Array()
		var uvs: PackedVector2Array = PackedVector2Array()
		
		var surface: Surface = Surface.new(vertices, uvs)
	return surface_vector_dict

func embed_2d_in_plane(v: Vector2, n: Vector3) -> Vector3:
	var a: Vector3
	n = n.normalized()
	if abs(n.z) < 0.9:
		a = Vector3(0, 0, 1)
	else:
		a = Vector3(0, 1, 0)
	var t1 = (a - n.dot(a) * n).normalized()
	var t2 = n.cross(t1)
	return a.x * t1 + a.y * t2

func build_chunk_mesh(
	x_pos_chunk: Chunk,
	x_neg_chunk: Chunk,
	y_pos_chunk: Chunk,
	y_neg_chunk: Chunk,
	z_pos_chunk: Chunk,
	z_neg_chunk: Chunk
):
	var exposed_block_surfaces = find_block_surfaces(
		x_pos_chunk,
		x_neg_chunk,
		y_pos_chunk,
		y_neg_chunk,
		z_pos_chunk,
		z_neg_chunk
	)
	var surfaces = get_surface_vectors(exposed_block_surfaces)
	
	var tmpMesh = Mesh.new()
	var vertices = PackedVector3Array()
	
	vertices.push_back(Vector3(1,0,0))
	vertices.push_back(Vector3(1,0,1))
	vertices.push_back(Vector3(0,0,1))
	vertices.push_back(Vector3(0,0,0))
	
	UVs.push_back(Vector2(0,0))
	UVs.push_back(Vector2(0,1))
	UVs.push_back(Vector2(1,1))
	UVs.push_back(Vector2(1,0))
	
	mat.albedo_color = color
	
	var st = SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLE_STRIP)
	st.set_material(mat)
	
	for v in vertices.size(): 
		st.add_color(color)
		st.add_uv(UVs[v])
		st.add_vertex(vertices[v])
	
	st.commit(tmpMesh)
	
	$MeshInstance.mesh = tmpMesh
