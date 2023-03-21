shader_type canvas_item;

global uniform vec2 mouse_position;

uniform sampler2D slotNormal : hint_default_white, filter_linear_mipmap;

uniform vec4 highlightColour : source_color = vec4(1.0);

uniform float mouseHeight : hint_range(0.0, 128.0, 0.1) = 10.0;
uniform float affectSize : hint_range(0.1, 512.0, 0.1) = 32.0;
uniform float fxStrengthPowerExponent : hint_range(0.0, 5.0, 0.1) = 2.0;

void fragment() {
	vec3 normal = (texture(slotNormal, UV).rgb) * 2.0 - 1.0;
	normal = normalize(normal);
	vec3 screenPos = vec3(FRAGCOORD.xy, 0.0);
	vec3 mouseDelta = screenPos - vec3(mouse_position, mouseHeight);
	float mouseDistance = length(mouseDelta.xy);
	float fxStrength = clamp(affectSize / mouseDistance, 0.0, 1.0);
	fxStrength = pow(fxStrength, fxStrengthPowerExponent);
	float amount = fxStrength * (dot(normalize(mouseDelta), normal) * 0.5 + 0.5);

	vec4 slotTex = texture(TEXTURE, UV);
	COLOR = mix(slotTex, highlightColour, amount);
	COLOR.a = slotTex.a;
	
	//COLOR = vec4(normal, 0.0, slotTex.a);
}
