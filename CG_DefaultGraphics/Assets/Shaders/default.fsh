#version 440 core

in vec3 _v;
in vec2 _vt;
in vec3 _vn;

uniform sampler2D tex;

out vec4 outColor;

void main(void)
{
	outColor = texture(tex, _vt);
}