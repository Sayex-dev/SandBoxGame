shader_type spatial;

uniform ivec2 px_block_texture_size;
uniform sampler2D block_map_texture : source_color, repeat_enable;
uniform vec2 atlas_texture_size; // full atlas size in pixels

void fragment() {
    // 1. Read block ID from UV2.x (assumes mesh has this)
    int block_id = int(UV2.x);
	// 2. Lookup the tile coordinate for this block ID.
	//    block_map_texture should be a 1D or 2D texture where each pixel encodes tile index.
	vec2 tile_index = texelFetch(block_map_texture, ivec2(block_id, 0), 0).rg;
	tile_index *= 255.0; // if stored in 0–1 range as bytes

	// 3. Compute UV offset inside atlas
	vec2 tile_px = tile_index * vec2(px_block_texture_size);
	vec2 block_uv_offset = tile_px / atlas_texture_size;
	vec2 block_uv_scale = vec2(px_block_texture_size) / atlas_texture_size;

	// 4. Final UV inside atlas
	vec2 final_uv = block_uv_offset + UV * block_uv_scale;

	// 5. Sample atlas
	ALBEDO = texture(TEXTURE, final_uv).rgb;
}