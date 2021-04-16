#version 440 core
layout(location = 0) in vec3 v;

uniform mat4 model;

void main()
{
    gl_Position = model * vec4(v, 1.0);
}