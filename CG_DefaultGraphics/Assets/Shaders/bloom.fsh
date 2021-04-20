#version 440 core
  
in vec2 _vt;

uniform sampler2D tex;
  
uniform bool horizontal;
uniform float weights[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);

out vec4 outColor;

void main()
{           
    vec2 tex_offset = 1.0 / textureSize(tex, 0);
    vec3 result = texture(tex, _vt).rgb * weights[0];
    if(horizontal)
    {
        for(int i = 1; i < 5; i++)
        {
            result += texture(tex, _vt + vec2(tex_offset.x * i, 0.0)).rgb * weights[i];
            result += texture(tex, _vt - vec2(tex_offset.x * i, 0.0)).rgb * weights[i];
        }
    }
    else
    {
        for(int i = 1; i < 5; i++)
        {
            result += texture(tex, _vt + vec2(0.0, tex_offset.y * i)).rgb * weights[i];
            result += texture(tex, _vt - vec2(0.0, tex_offset.y * i)).rgb * weights[i];
        }
    }
    outColor = vec4(result, 1.0);
}