extends Node
class_name WorldController

class Chunk:
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
		return _blocks[index]
	
	func set_block(chunk_pos: Vector3i, block_id: int):
		var index: int = chunk_to_array_pos(chunk_pos)
		_blocks[index] = block_id
		
	func chunk_to_array_pos(chunk_pos: Vector3i) -> int:
		var index: int = chunk_pos.x + chunk_pos.y * _chunk_size.x + chunk_pos.z * _chunk_size.x * _chunk_size.y
		assert (index > _chunk_size.x * _chunk_size.y * _chunk_size.z, "Position not inside chunk bounds.")
		return index
	
	func array_to_chunk_pos(index: int) -> Vector3i:
		assert (index > _chunk_size.x * _chunk_size.y * _chunk_size.z, "Position not inside chunk bounds.")
		var x = index % _chunk_size.x
		var y = floori(index / _chunk_size.x) % _chunk_size.y
		var z = floori(index / _chunk_size.x * chunk_size.y)
		return Vector3i(x, y, z)

class BlockWorld:
	var loaded_chunks: Dictionary

	func add_chunk(chunk_index: Vector3i, chunk: Chunk):
		assert (chunk_index not in loaded_chunks, "Chunk already exists and cannot be overwritten.")
		loaded_chunks[chunk_index] = chunk

	func remove_chunk(chunk_index: Vector3i):
		loaded_chunks.erase(chunk_index)

var block_world: BlockWorld
var world_mesh: MeshInstance3D



var UVs = PackedVector2Array()
var mat = StandardMaterial3D.new()
var color = Color(0.9, 0.1, 0.1)

func _find_start_pos_in_chunk(chunk: Chunk):


func _build_chunk_mesh(chunk: Chunk):
	# Find start position
	var start_pos
	for i in range(len(chunk._blocks)):
		if chunk._blocks[i] > -1:
			start_pos = chunk.array_to_chunk_pos(i)
			break
	
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
