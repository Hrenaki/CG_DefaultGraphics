#version 440 core
  
in vec2 _vt;

uniform sampler2D tex;
uniform float near;
uniform float far;

out vec4 outColor;

float linearizeDepth(float depth)
{
    float z = depth * 2.0 - 1.0;
    return (2.0 * near * far) / (far + near - z * (far - near));
}

void main()
{
    //outColor = vec4(vec3(linearizeDepth(texture(tex, _vt).r) / far), 1.0f);
    outColor = vec4(vec3(texture(tex, _vt).r), 1.0f);
}