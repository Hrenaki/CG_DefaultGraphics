#version 440 core

in vec3 _v;
in vec2 _vt;
in vec3 _vn;

uniform sampler2D tex;

#define MAX_LIGHTS_COUNT 64
struct Light
{
    vec4 position;
    vec4 direction;
    vec4 coeffs;
    vec3 color;
    samplerCubeShadow shadow;
};
uniform Light lights[MAX_LIGHTS_COUNT];
uniform int lightsCount;
uniform vec3 camPos;

out vec4 outColor;

void main()
{
    vec3 lightColor = vec3(0);
    float curLightCoef;

    for (int i = 0; i < lightsCount; i++)
    {
        curLightCoef = 0.0f;

        if (lights[i].position.w == 0.0f)
        {
            if (lights[i].direction.w == 0.0f) // ambient light
                curLightCoef = lights[i].coeffs.y;
            else                             // directional light
            {
                vec3 lightDirection = -lights[i].direction.xyz;
                curLightCoef += max(0.0f, dot(lightDirection, _vn));
                curLightCoef += pow(max(0.0f, dot(normalize(camPos - _v), reflect(-lightDirection, _vn))), 128.0f);
                curLightCoef *= lights[i].coeffs.y;
            }
        }
        else
        {
            float dist = distance(lights[i].position.xyz, _v);
            if (dist > lights[i].coeffs.x)
                continue;
            if (lights[i].direction.w == 0.0f) // point light
            {
                vec3 lightDirection = normalize(lights[i].position.xyz - _v);
                curLightCoef += max(0.0f, dot(lightDirection, _vn));
                curLightCoef += pow(max(0.0f, dot(normalize(camPos - _v), reflect(-lightDirection, _vn))), 128.0f);
            }
            else                             // spot light
            {
                vec3 lightDirection = normalize(lights[i].position.xyz - _v);
                if (dot(lightDirection, -lights[i].direction.xyz) >= cos(lights[i].coeffs.w))
                {
                    curLightCoef += max(0.0f, dot(lightDirection, _vn));
                    curLightCoef += pow(max(0.0f, dot(normalize(camPos - _v), reflect(-lightDirection, _vn))), 128.0f);
                }
            }
            curLightCoef *= lights[i].coeffs.y * pow((lights[i].coeffs.x - dist) / lights[i].coeffs.x, (4.0f - 3.0f * lights[i].coeffs.z) / 2.0f);
        }

        lightColor += lights[i].color * curLightCoef;
    }
    vec3 baseColor = texture(tex, _vt).rgb;
    outColor = vec4(baseColor * lightColor, 1.0f);
}
//#version 440 core
//
//in vec3 _v;
//in vec2 _vt;
//in vec3 _vn;
//
//uniform sampler2D tex;
//
//#define MAX_LIGHTS_COUNT 64
//uniform vec4 lights_position[MAX_LIGHTS_COUNT];
//uniform vec4 lights_direction[MAX_LIGHTS_COUNT];
//uniform vec4 lights_coeffs[MAX_LIGHTS_COUNT]; // radius, brightness, intensity, angle
//uniform vec3 lights_color[MAX_LIGHTS_COUNT];
//uniform int lightsCount;
//uniform vec3 camPos;
//
//out vec4 outColor;
//
//void main()
//{
//    vec3 lightColor = vec3(0);
//    float curLightCoef;
//
//    for (int i = 0; i < lightsCount; i++)
//    {
//        curLightCoef = 0.0f;
//
//        if (lights_position[i].w == 0.0f)
//        {
//            if (lights_direction[i].w == 0.0f) // ambient light
//                curLightCoef = lights_coeffs[i].y;
//            else                             // directional light
//            {
//                vec3 lightDirection = -lights_direction[i].xyz;
//                curLightCoef += max(0.0f, dot(lightDirection, _vn));
//                curLightCoef += pow(max(0.0f, dot(normalize(camPos - _v), reflect(-lightDirection, _vn))), 128.0f);
//                curLightCoef *= lights_coeffs[i].y;
//            }
//        }
//        else
//        {
//            float dist = distance(lights_position[i].xyz, _v);
//            if (dist > lights_coeffs[i].x)
//                continue;
//            if (lights_direction[i].w == 0.0f) // point light
//            {
//                vec3 lightDirection = normalize(lights_position[i].xyz - _v);
//                curLightCoef += max(0.0f, dot(lightDirection, _vn));
//                curLightCoef += pow(max(0.0f, dot(normalize(camPos - _v), reflect(-lightDirection, _vn))), 128.0f);
//            }
//            else                             // spot light
//            {
//                vec3 lightDirection = normalize(lights_position[i].xyz - _v);
//                if (dot(lightDirection, -lights_direction[i].xyz) >= cos(lights_coeffs[i].w))
//                {
//                    curLightCoef += max(0.0f, dot(lightDirection, _vn));
//                    curLightCoef += pow(max(0.0f, dot(normalize(camPos - _v), reflect(-lightDirection, _vn))), 128.0f);
//                }
//            }
//            curLightCoef *= lights_coeffs[i].y * pow((lights_coeffs[i].x - dist) / lights_coeffs[i].x, (4.0f - 3.0f * lights_coeffs[i].z) / 2.0f);
//        }
//
//        lightColor += lights_color[i] * curLightCoef;
//    }
//    vec3 baseColor = texture(tex, _vt).rgb;
//    outColor = vec4(baseColor * lightColor, 1.0f);
//}