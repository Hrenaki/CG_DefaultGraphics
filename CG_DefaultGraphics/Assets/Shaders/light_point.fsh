#version 440 core

in vec3 _v;

uniform vec3 lightPos;
uniform float radius;

void main()
{
    gl_FragDepth = length(_v - lightPos) / radius;
}  