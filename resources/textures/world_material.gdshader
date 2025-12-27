shader_type spatial;
render_mode blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx;

uniform float blockSize;
uniform vec4 albedo : source_color;
uniform sampler2D texture_albedo : source_color,filter_nearest,repeat_enable;

uniform vec2 atlas_size;
varying vec2 offset;

void vertex() {
	offset = COLOR.xy;
}

void fragment() {
	vec2 base_uv = UV;

	// Mod the uv and add offsetdw
	vec2 mod_uv = vec2(mod(base_uv / atlas_size, vec2(1.000, 1.000) / atlas_size)) + ((offset * 256.0) / atlas_size);

	vec4 albedo_tex = texture(texture_albedo,mod_uv);
	ALBEDO = albedo.rgb * albedo_tex.rgb;
}