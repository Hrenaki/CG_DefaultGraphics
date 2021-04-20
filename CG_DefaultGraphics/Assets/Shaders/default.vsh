#version 440 core
#define MAX_LIGHTS_COUNT 8

layout(location = 0) in vec3 v;
layout(location = 1) in vec2 vt;
layout(location = 2) in vec3 vn;

uniform mat4 model;
uniform mat4 camSpace;

struct AmbientLight
{
    float brightness;
    vec3 color;
};

struct DirectionalLight
{
    vec3 direction;
    float radius;
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

out vec3 _v;
out vec2 _vt;
out vec3 _vn;
out vec4 _vdl[MAX_LIGHTS_COUNT];
out vec4 _vsl[MAX_LIGHTS_COUNT];

void main(void)
{
	gl_Position = camSpace * model * vec4(v, 1.0f);
	_v = (model * vec4(v, 1.0f)).xyz;
	_vt = vt;
	_vn = vn;
    for (int i = 0; i < directionalLightsCount; i++)
        _vdl[i] = directionalLights[i].lightSpace * model * vec4(v, 1.0f);
    for (int i = 0; i < spotLightsCount; i++)
        _vsl[i] = spotLights[i].lightSpace * model * vec4(v, 1.0f);
}