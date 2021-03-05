#version 440 core

in vec3 _v;
in vec2 _vt;
in vec3 _vn;

uniform sampler2D tex;
uniform vec4 lights[64];
uniform vec4 lightsCoefs[64];
uniform int lightsCount;
uniform float ambient;

out vec4 outColor;

void main(void)
{
    
    if (lightsCount == 0)
    {
        outColor = vec4(1.0f, 1.0f, 1.0f, 1.0f);
        return;
    }
    float lightCoef = ambient;
    for (int i = 0; i < lightsCount; i++)
    {
        float coef_normal = 0.0f;
        float coef_glare = 0.0f;

        lightCoef += lights[i].x;
        lightCoef -= lights[i].x;
        lightCoef += (1.0f - lightCoef) * lightsCoefs[i].x;

        //if (lights[i].w == 0.0f)
        //{
        //    vec3 lightDirection = lights[i].xyz;
        //    lightCoef += (1.0f - lightCoef) * max(0.0f, dot(_vn, lightDirection));
        //}
        //else if (lights[i].w == 1.0f)
        //{
        //    vec3 lDir = normalize(lights[i].xyz - _v);
        //    vec3 vDir = normalize(-_v);
        //    float dist = distance(lights[i].xyz, _v);
        //    //coef_normal = max(0.0f, dot(_vn, lDir));
        //    coef_normal = max(0.0f, 1.0f -
        //                   lightsCoefs[i].x -
        //                   lightsCoefs[i].y * dist -
        //                   lightsCoefs[i].z * dist * dist);
        //    vec3 reflection = 2.0f * dot(_vn, lDir) * _vn - lDir;
        //    float glare_k = max(0.0f, dot(reflection, vDir) - 0.95) * 20.0f;
        //    coef_glare += coef_normal * glare_k * glare_k * lightsCoefs[i].w * (1.0f - dot(_vn, lDir));
        //}
        //lightCoef += (1.0f - lightCoef) * coef_normal;
        //lightCoef += (1.0f - lightCoef) * coef_glare;
    }
    vec3 baseColor = vec3(1.0f, 1.0f, 1.0f);
    outColor = vec4(baseColor * lightCoef, 1.0f);
}