#version 440 core

layout(triangles) in;
layout(triangle_strip, max_vertices = 18) out;

uniform mat4 lightSpaces[6];

out vec3 _v;

void main()
{
    for (int face = 0; face < 6; face++)
    {
        gl_Layer = face; // built-in variable that specifies to which face we render.
        for (int i = 0; i < 3; i++) // for each triangle vertex
        {
            _v = gl_in[i].gl_Position.xyz;
            gl_Position = lightSpaces[face] * vec4(_v, 1.0f);
            EmitVertex();
        }
        EndPrimitive();
    }
}