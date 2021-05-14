#version 440 core
#define MAX_LIGHTS_COUNT 8
#define SHADOW_BIAS_MIN 0.001f
#define SHADOW_BIAS_MAX 0.005f

in vec3 _v;
in vec2 _vt;
in vec3 _vn;
in vec4 _vdl[MAX_LIGHTS_COUNT];
in vec4 _vsl[MAX_LIGHTS_COUNT];

struct Material
{
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float metallic;
};
uniform Material material;
uniform sampler2D tex;

struct AmbientLight
{
    float brightness;
    vec3 color;
};

struct DirectionalLight
{
    vec3 direction;
    float brightness;
    vec3 color;
    mat4 lightSpace;
    sampler2D shadowTex;
};

struct SpotLight
{
    vec3 position;
    vec3 direction;
    float radius;
    float brightness;
    float intensity;
    float angle;
    vec3 color;
    mat4 lightSpace;
    sampler2D shadowTex;
};

struct PointLight
{
    vec3 position;
    float radius;
    float brightness;
    float intensity;
    vec3 color;
    samplerCube shadowCube;
};
uniform AmbientLight ambientLights[MAX_LIGHTS_COUNT];
uniform int ambientLightsCount;
uniform DirectionalLight directionalLights[MAX_LIGHTS_COUNT];
uniform int directionalLightsCount;
uniform SpotLight spotLights[MAX_LIGHTS_COUNT];
uniform int spotLightsCount;
uniform PointLight pointLights[MAX_LIGHTS_COUNT];
uniform int pointLightsCount;

uniform vec3 camPos;
uniform float spot_near;

layout (location = 0) out vec4 outColor;
layout (location = 1) out vec4 bloomColor;

void main()
{
    vec3 lightColor = vec3(0);
    vec3 curLightColor;

    for (int i = 0; i < ambientLightsCount; i++)
    {
        curLightColor = ambientLights[i].brightness * material.ambient;
        lightColor += ambientLights[i].color * curLightColor;
    }
    for (int i = 0; i < directionalLightsCount; i++)
    {
        curLightColor = vec3(0.0f);
    
        vec3 vl = (_vdl[i].xyz / _vdl[i].w) * 0.5f + 0.5f;
        vec3 lightDirection = -directionalLights[i].direction;
        if (vl.z - max(SHADOW_BIAS_MAX * (1.0f - dot(_vn, lightDirection)), SHADOW_BIAS_MIN) <= texture(directionalLights[i].shadowTex, vl.xy).r)
        {
            curLightColor += max(0.0f, dot(lightDirection, _vn)) * material.diffuse;
            curLightColor += pow(max(0.0f, dot(_vn, normalize(lightDirection + normalize(camPos - _v)))), material.metallic) * material.specular;
            curLightColor *= directionalLights[i].brightness;
        }
        lightColor += directionalLights[i].color * curLightColor; 
    }
    for (int i = 0; i < spotLightsCount; i++)
    {
        curLightColor = vec3(0.0f);
    
        vec3 lightDirection = normalize(spotLights[i].position - _v);
        if (dot(lightDirection, -spotLights[i].direction) >= cos(spotLights[i].angle / 2.0f))
        {
            vec3 vl = (_vsl[i].xyz / _vsl[i].w) * 0.5f + 0.5f;
            if ((2.0 * spot_near) / (spotLights[i].radius + spot_near - (vl.z * 2.0f - 1.0f) * (spotLights[i].radius - spot_near)) - 
                max(SHADOW_BIAS_MAX * (1.0f - dot(_vn, lightDirection)), SHADOW_BIAS_MIN) <=
                (2.0 * spot_near) / (spotLights[i].radius + spot_near - (texture(spotLights[i].shadowTex, vl.xy).r * 2.0f - 1.0f) * (spotLights[i].radius - spot_near)))
            {
                float dist = distance(spotLights[i].position, _v);
    
                curLightColor += max(0.0f, dot(lightDirection, _vn)) * material.diffuse;
                curLightColor += pow(max(0.0f, dot(_vn, normalize(lightDirection + normalize(camPos - _v)))), material.metallic) * material.specular;
                curLightColor *= pow(spotLights[i].brightness * (spotLights[i].radius - dist) / spotLights[i].radius, (4.0f - 3.0f * spotLights[i].intensity) / 2.0f);
            }
        }
        
        lightColor += spotLights[i].color * curLightColor; 
    }
    for (int i = 0; i < pointLightsCount; i++)
    {
        curLightColor = vec3(0.0f);
        
        vec3 lightVec = _v - pointLights[i].position;
        vec3 lightDirection = -normalize(lightVec);
        float dist = length(lightVec);
        if (dist - max(SHADOW_BIAS_MAX * (1.0f - dot(_vn, -normalize(lightVec))), SHADOW_BIAS_MIN) <= 
            texture(pointLights[i].shadowCube, lightVec).r * pointLights[i].radius)
        {
            curLightColor += max(0.0f, dot(lightDirection, _vn)) * material.diffuse;
            curLightColor += pow(max(0.0f, dot(_vn, normalize(lightDirection + normalize(camPos - _v)))), material.metallic) * material.specular;
            curLightColor *= pow(pointLights[i].brightness * (pointLights[i].radius - dist) / pointLights[i].radius, (4.0f - 3.0f * pointLights[i].intensity) / 2.0f);
        }

        lightColor += pointLights[i].color * curLightColor;
    }
    vec3 baseColor = texture(tex, _vt).rgb;
    outColor = vec4(baseColor * lightColor, 1.0f);
    bloomColor = length(outColor.rgb) > 1.0f ? outColor : vec4(0f);
}