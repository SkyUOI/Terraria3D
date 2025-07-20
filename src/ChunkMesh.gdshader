shader_type spatial;
render_mode blend_mix, cull_back;

// 你的图集
uniform sampler2D atlas : source_color;
uniform vec2 atlas_size;

// 顶点数据
// INSTANCE_CUSTOM.xyzw = vec4(u0, v0, w, h) 已经由 MultiMesh 传进来

void vertex() {
    // 把本地 Quad UV (0~1) 映射到图集中对应矩形
    vec4 rect = INSTANCE_CUSTOM;

    // 归一化到 0~1 空间
    vec2 uv0 = vec2(rect.x, rect.y) / atlas_size;
    vec2 uv1 = uv0 + vec2(rect.z, rect.w) / atlas_size;

    // 翻转 Y（OpenGL 纹理坐标系）
    UV = mix(uv0, uv1, UV);
}

void fragment() {
    ALBEDO = texture(atlas, UV).rgb;
}