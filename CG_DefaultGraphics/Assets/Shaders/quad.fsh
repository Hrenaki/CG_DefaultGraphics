#version 440 core
  
in vec2 _vt;

uniform sampler2D depthMap;
uniform float near;
uniform float far;

out vec4 outColor;

//float LinearizeDepth(float depth)
//{
//    float z = depth * 2.0 - 1.0; // Back to NDC 
//    return (2.0 * near * far) / (far + near - z * (far - near));
//}

void main()
{         
    float depthValue = texture(depthMap, _vt).r;
    outColor = vec4(vec3((2.0 * near) / (far + near - (depthValue * 2.0f - 1.0f) * (far - near))), 1.0);
}  