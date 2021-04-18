#version 440 core
layout(location = 0) in vec2 v;
layout(location = 1) in vec2 vt;

out vec2 _vt;

void main()
{
    gl_Position = vec4(v, 0.0f, 1.0f);
    _vt = vt;
}