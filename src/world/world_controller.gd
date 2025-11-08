extends Node
class_name WorldController


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
