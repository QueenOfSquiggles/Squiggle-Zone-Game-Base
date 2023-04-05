shader_type canvas_item;

uniform float colour_depth = 32.0;
uniform float dither_uv_scale : hint_range(0.0, 50.0) = 1.0;
uniform float dither_range : hint_range(0.0, 1.0) = 1.0;
uniform vec2 dither_uv_time_scale = vec2(0.01);

uniform sampler2D dither_texture : hint_default_white, repeat_enable, filter_linear;


uniform sampler2D SCREEN_COLOUR : hint_screen_texture, repeat_enable, filter_linear;

void fragment() 
{
	vec2 dither_size = vec2(textureSize(dither_texture, 0));
	vec2 buffer_size = vec2(textureSize(SCREEN_COLOUR, 0));
	
	vec4 colour = texture(SCREEN_COLOUR, SCREEN_UV);
	vec2 dither_uv_offset = dither_uv_time_scale * TIME;
	vec3 dither = texture(dither_texture, SCREEN_UV * (buffer_size / dither_size) * dither_uv_scale + dither_uv_offset).rgb;
	dither -= 0.5;
	
	colour.rgb = round(colour.rgb * colour_depth + (dither_range * dither)) / colour_depth;
	COLOR.rgb = colour.rgb;
	
}
