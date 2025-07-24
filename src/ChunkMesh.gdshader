shader_type spatial;
render_mode blend_mix, cull_back;

uniform sampler2D atlas : source_color;

void vertex() {
    vec4 rect = INSTANCE_CUSTOM;

    vec2 uv0 = vec2(rect.x, rect.y);
    vec2 uv1 = uv0 + vec2(rect.z, rect.w);

    UV = mix(uv0, uv1, UV);
}

void fragment() {
    ALBEDO = texture(atlas, UV).rgb;
}