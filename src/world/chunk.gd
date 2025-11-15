extends MeshInstance3D

class_name Chunk 

class Surface:
	var vertices: Array[Vector3i] = []
	var normal: Vector3
	
	func _init(_vertices: Array[Vector3i], _normal: Vector3):
		vertices = _vertices
		normal = _normal

var normals = [
	Vector3i(1, 0, 0),	# x+
	Vector3i(-1, 0, 0),	# x-
	Vector3i(0, 1, 0),	# y+
	Vector3i(0, -1, 0),	# y-
	Vector3i(0, 0, 1),	# z+
	Vector3i(0, 0, -1)	# z-
]
var chunk_size: Vector3i
var blocks: Array[int]

func _init(_chunk_size: Vector3i) -> void:
	chunk_size = _chunk_size
	blocks = []
	var block_count = _chunk_size.x * _chunk_size.y * _chunk_size.z
	blocks.resize(block_count)
	blocks.fill(-1)

func get_block(chunk_pos: Vector3i) -> int:
	var index: int = chunk_to_array_pos(chunk_pos)
	return blocks[index]

func set_block(chunk_pos: Vector3i, block_id: int):
	var index: int = chunk_to_array_pos(chunk_pos)
	blocks[index] = block_id
	
func chunk_to_array_pos(chunk_pos: Vector3i) -> int:
	var index: int = chunk_pos.x + chunk_pos.y * chunk_size.x + chunk_pos.z * chunk_size.x * chunk_size.y
	return index

func array_to_chunk_pos(index: int) -> Vector3i:
	var x = index % chunk_size.x
	@warning_ignore("integer_division")
	var y = int(index / chunk_size.x) % chunk_size.y
	@warning_ignore("integer_division")
	var z = int(index / chunk_size.x) * chunk_size.y
	return Vector3i(x, y, z)

func is_in_chunk(index: int) -> bool:
	return 0 >= index and index < (chunk_size.x * chunk_size.y * chunk_size.z)

func find_block_surfaces(
	x_pos_chunk: Chunk = null,
	x_neg_chunk: Chunk = null,
	y_pos_chunk: Chunk = null,
	y_neg_chunk: Chunk = null,
	z_pos_chunk: Chunk = null,
	z_neg_chunk: Chunk = null
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
	for z in range(chunk_size.z):
		for y in range(chunk_size.y):
			for x in range(chunk_size.x):
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
						var adjacent_chunk_pos = adjacent_pos % chunk_size
						if adjacent_chunks[i]:
							adjacent_block = adjacent_chunks[i].get_block(adjacent_chunk_pos)
						else:
							adjacent_block = -1
					if adjacent_block == -1:
						has_exposed_surfaces = true
						exposed_surfaces[i] = true
				if has_exposed_surfaces:
					exposed_blocks[block_pos] = exposed_surfaces
	return exposed_blocks

func get_surface_vectors(exposed_block_surfaces: Dictionary) -> Array[Surface]:
	var block_surfaces = {}
	for pos in exposed_block_surfaces:
		block_surfaces[pos] = exposed_block_surfaces[pos].duplicate()
	
	var max_dim: int = max(chunk_size.x, chunk_size.y, chunk_size.z)
	var surfaces: Array[Surface] = []
	while len(block_surfaces) > 0:
		# Find a surface on a block
		# Example:
		# [ ][ ][ ]
		#    [X][ ]
		# [ ][ ][ ]
		var start_pos: Vector3i = block_surfaces.keys()[0]
		var dir_i: int
		var normal: Vector3i
		for normal_i in range(len(normals)):
			if block_surfaces[start_pos][normal_i]:
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
		var max_y: int = -1
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
				max_y = y - 1
				break
		
		# Remove surfaces that have been used up
		for z in range(min_pos.z, max_pos.z + 1):
			for y in range(min_pos.y, max_pos.y + 1):
				for x in range(min_pos.x, max_pos.x + 1):
					var remove_pos = Vector3i(x, y, z)
					block_surfaces[remove_pos][dir_i] = false
					if not block_surfaces[remove_pos].has(true):
						block_surfaces.erase(remove_pos)
		
		# Find corner vectors of surface in world space
		var vertices: Array[Vector3i] = []
		
		var displacement: Vector3i = (Vector3(normal) * 0.5) + (Vector3(normal.abs()) * 0.5)
		var base_pos: Vector3i = min_pos + displacement
		var corner_1: Vector3i = Vector3i(embed_2d_in_plane(Vector2i(0, 0), normal)) + base_pos
		var corner_2: Vector3i = Vector3i(embed_2d_in_plane(Vector2i(0, max_y + 1), normal)) + base_pos
		var corner_3: Vector3i = Vector3i(embed_2d_in_plane(Vector2i(max_x + 1, max_y + 1), normal)) + base_pos
		var corner_4: Vector3i = Vector3i(embed_2d_in_plane(Vector2i(max_x + 1, 0), normal)) + base_pos
		
		# Triangle 1
		vertices.append(corner_1)
		vertices.append(corner_2)
		vertices.append(corner_3)
		
		# Triangle 2
		vertices.append(corner_3)
		vertices.append(corner_4)
		vertices.append(corner_1)
		
		var surface: Surface = Surface.new(vertices, normal)
		surfaces.append(surface)
	return surfaces

func embed_2d_in_plane(v: Vector2, n: Vector3) -> Vector3:
	var a: Vector3
	n = n.normalized()
	if abs(n.z) < 0.9:
		a = Vector3(0, 0, 1)
	else:
		a = Vector3(0, 1, 0)
	var t1 = (a - n.dot(a) * n).normalized()
	var t2 = n.cross(t1)
	return v.x * t1 + v.y * t2

func build_chunk_mesh(
	x_pos_chunk: Chunk = null,
	x_neg_chunk: Chunk = null,
	y_pos_chunk: Chunk = null,
	y_neg_chunk: Chunk = null,
	z_pos_chunk: Chunk = null,
	z_neg_chunk: Chunk = null
):
	var exposed_block_surfaces = find_block_surfaces(
		x_pos_chunk,
		x_neg_chunk,
		y_pos_chunk,
		y_neg_chunk,
		z_pos_chunk,
		z_neg_chunk
	)
	var surfaces: Array[Surface] = get_surface_vectors(exposed_block_surfaces)
	
	var st = SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	st.set_color(Color.AQUA)
	
	for i in len(surfaces):
		var surface: Surface = surfaces[i]
		st.set_normal(surface.normal)
		# Add vertices
		for vertex in surface.vertices:
			st.add_vertex(vertex)
	st.index()
	
	mesh = st.commit()
