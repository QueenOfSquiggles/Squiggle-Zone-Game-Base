shader_type spatial;
render_mode unshaded, cull_disabled;

uniform sampler2D DEPTH_TEXTURE : hint_depth_texture, filter_linear;
uniform sampler2D SCREEN_COLOUR : hint_screen_texture, filter_linear;


varying mat4 CAMERA_MATRIX;

void vertex()
{
	POSITION = vec4(VERTEX, 1.0); // forces transform to fill screen
	CAMERA_MATRIX = INV_VIEW_MATRIX;
}

void fragment() 
{
	float depth = texture(DEPTH_TEXTURE, SCREEN_UV).x;
	vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth);
	vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
	view.xyz /= view.w;
	float linear_depth = -view.z;
	vec4 world = CAMERA_MATRIX * INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
	vec3 world_position = world.xyz / world.w;

	vec4 screen_col = texture(SCREEN_COLOUR, SCREEN_UV);
	ALBEDO = vec3(1.0 - (linear_depth / 8.0));
	if (linear_depth >= 2048.0) // bad because at 2048 meters effect cuts out
	{
		discard;
	}
}
