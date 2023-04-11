shader_type canvas_item;

uniform float colour_depth = 32.0;
uniform float dither_uv_scale : hint_range(0.0, 50.0) = 1.0;
uniform float dither_range : hint_range(0.0, 1.0) = 1.0;


uniform sampler2D dither_texture : hint_default_white, repeat_enable, filter_nearest;

void fragment() 
{
	vec2 dither_size = vec2(textureSize(dither_texture, 0));
	vec2 buffer_size = vec2(textureSize(TEXTURE, 0));
	
	vec4 colour = texture(TEXTURE, UV);
	vec3 dither = texture(dither_texture, UV * (buffer_size / dither_size) * dither_uv_scale).rgb;
	dither -= 0.5;
	
	colour.rgb = round(colour.rgb * colour_depth + (dither_range * dither)) / colour_depth;
	COLOR.rgb = colour.rgb;
	
}
