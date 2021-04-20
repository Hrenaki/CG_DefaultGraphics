#version 440 core
  
in vec2 _vt;

uniform sampler2D tex;
uniform sampler2D bloomTex;
uniform float exposure;

out vec4 outColor;

void main()
{         
    vec3 color = texture(tex, _vt).rgb + texture(bloomTex, _vt).rgb * 0.1f;
    outColor = vec4(pow(vec3(1.0f) - exp(-color * exposure), vec3(1.0f / 2.2f)), 1.0f);
}  