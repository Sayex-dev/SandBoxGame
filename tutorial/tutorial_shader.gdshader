shader_type spatial;
render_mode specular_toon;

uniform float height_scale = 0.5;
uniform sampler2D noise;
uniform sampler2D normalmap;

varying vec2 tex_position;

float wave(vec2 position){
  position += texture(noise, position / 10.0).x * 2.0 - 1.0;
  vec2 wv = 1.0 - abs(sin(position));
  return pow(1.0 - pow(wv.x * wv.y, 0.65), 4.0);
}

float height(vec2 position, float time) {
	float time_slowed = time * 0.2;
	float d = wave((position + time_slowed) * 0.4) * 0.3;
	d += wave((position - time_slowed) * 0.3) * 0.3;
	d += wave((position + time_slowed) * 0.5) * 0.2;
	d += wave((position - time_slowed) * 0.6) * 0.2;
	return d * height_scale;
}

void vertex() {
	vec2 pos = VERTEX.xz;
	float k = height(pos, TIME);
	NORMAL = normalize(vec3(k - height(pos + vec2(0.1, 0.0), TIME), 0.1, k - height(pos + vec2(0.0, 0.1), TIME)));
	VERTEX.y += k;
}

void fragment() {
	NORMAL_MAP = texture(normalmap, tex_position).xyz;
	float fresnel = sqrt(1.0 - dot(NORMAL, VIEW));
	RIM = 0.2;
	METALLIC = 0.0;
	ROUGHNESS = 0.01 * (1.0 - fresnel);
	ALBEDO = vec3(0.01, 0.03, 0.05) + (0.1 * fresnel);
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
