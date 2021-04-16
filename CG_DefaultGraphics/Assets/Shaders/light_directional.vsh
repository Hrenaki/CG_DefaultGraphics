#version 440 core
layout(location = 0) in vec3 v;
layout(location = 1) in vec2 vt;
layout(location = 2) in vec3 vn;

uniform mat4 model;
uniform mat4 lightSpace;

void main()
{
    gl_Position = lightSpace * model * vec4(v, 1.0);
}