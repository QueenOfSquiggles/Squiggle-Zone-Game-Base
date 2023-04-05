shader_type spatial;
render_mode unshaded, cull_disabled;

uniform float world_uv_scale = 0.1;
uniform float posterize_levels = 8.0;
uniform float dither_bias = 0.01;
uniform vec3 noise_scroll_a = vec3(1.0);
uniform vec3 noise_scroll_b = vec3(-1.0);
uniform sampler2D screen_blue_noise_a : hint_default_white, filter_linear_mipmap;
uniform sampler2D screen_blue_noise_b : hint_default_white, filter_linear_mipmap;

uniform sampler2D DEPTH_TEXTURE : hint_depth_texture, filter_linear;
uniform sampler2D SCREEN_COLOUR : hint_screen_texture, filter_linear;

varying mat4 CAMERA_MATRIX;

void vertex()
{
	POSITION = vec4(VERTEX, 1.0); // forces transform to fill screen
	CAMERA_MATRIX = INV_VIEW_MATRIX;
}

float luma(vec3 col)
{
	return (col.r * 0.212) + (col.g * 0.701) + (col.b * 0.087);
}
vec3 posterize(vec3 col, float levels)
{
	float grey = luma(col);
	float lower = floor(grey * levels) / levels;
	float lowerDiff = abs(grey - lower);
	float upper = ceil(grey * levels) / levels;
	float upperDiff = abs(upper - grey);
	float level = lowerDiff <= upperDiff ? lower : upper;
	float adjustment = level * grey;
	return col * adjustment;
}


void fragment() 
{
	// generate depth information
	float depth = texture(DEPTH_TEXTURE, SCREEN_UV).x;
	vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth);
	vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
	view.xyz /= view.w;
	float linear_depth = -view.z;
	vec4 world = CAMERA_MATRIX * INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
	vec3 world_position = world.xyz / world.w;
	
	if (linear_depth > 2048.0)
	{
		// do not affect long distance depth
		discard;
	}

	// calculate world-space noise sampling
	vec2 scroll_pos_a = SCREEN_UV + (noise_scroll_a.rg * TIME);
	vec2 scroll_pos_b = SCREEN_UV + (noise_scroll_b.gb * TIME);
	vec4 noise_sample_a = texture(screen_blue_noise_a, scroll_pos_a * world_uv_scale);
	vec4 noise_sample_b = texture(screen_blue_noise_b, scroll_pos_b * world_uv_scale);
	
	// simple merge of noise affect
	float noise_affect = noise_sample_a.r * noise_sample_b.r;
	
	
	
	vec4 screen_col = texture(SCREEN_COLOUR, SCREEN_UV);
	float l = luma(screen_col.rgb) + dither_bias;
	if (l > noise_affect)
	{
 		vec3 colour_stack = posterize(screen_col.rgb, posterize_levels);
		ALBEDO = colour_stack;
	} else {
		ALBEDO = vec3(0.0);
	}	
	
	
}
