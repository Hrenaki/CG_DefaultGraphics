#version 440 core
#define MAX_LIGHTS_COUNT 8
#define SHADOW_BIAS_MIN 0.001f
#define SHADOW_BIAS_MAX 0.005f

in vec3 _v;
in vec2 _vt;
in vec3 _vn;
in vec4 _vdl[MAX_LIGHTS_COUNT];
in vec4 _vsl[MAX_LIGHTS_COUNT];

uniform sampler2D tex;

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

//struct Light
//{
//    vec4 position;
//    vec4 direction;
//    vec4 coeffs;
//    vec3 color;
//	mat4 lightSpace[6];
//    sampler2D shadowTex;
//    //samplerCube shadowCube;
//};
//uniform Light lights[MAX_LIGHTS_COUNT];

uniform vec3 camPos;
uniform float spot_near;

layout (location = 0) out vec4 outColor;

void main()
{
    vec3 lightColor = vec3(0);
    float curLightCoef;

    for (int i = 0; i < ambientLightsCount; i++)
    {
        curLightCoef = ambientLights[i].brightness;
        lightColor += ambientLights[i].color * curLightCoef;        
    }
    for (int i = 0; i < directionalLightsCount; i++)
    {
        curLightCoef = 0f;
    
        vec3 vl = (_vdl[i].xyz / _vdl[i].w) * 0.5f + 0.5f;
        vec3 lightDirection = -directionalLights[i].direction;
        if (vl.z - max(SHADOW_BIAS_MAX * (1.0f - dot(_vn, lightDirection)), SHADOW_BIAS_MIN) <= texture(directionalLights[i].shadowTex, vl.xy).r)
        {
            curLightCoef += max(0.0f, dot(lightDirection, _vn));
            curLightCoef += pow(max(0.0f, dot(_vn, normalize(lightDirection + normalize(camPos - _v)))), 128f);
            curLightCoef *= directionalLights[i].brightness;
        }
        lightColor += directionalLights[i].color * curLightCoef; 
    }
    for (int i = 0; i < spotLightsCount; i++)
    {
        curLightCoef = 0f;
    
        vec3 lightDirection = normalize(spotLights[i].position - _v);
        if (dot(lightDirection, -spotLights[i].direction) >= cos(spotLights[i].angle / 2.0f))
        {
            vec3 vl = (_vsl[i].xyz / _vsl[i].w) * 0.5f + 0.5f;
            if ((2.0 * spot_near) / (spotLights[i].radius + spot_near - (vl.z * 2.0f - 1.0f) * (spotLights[i].radius - spot_near)) - 
                max(SHADOW_BIAS_MAX * (1.0f - dot(_vn, lightDirection)), SHADOW_BIAS_MIN) <=
                (2.0 * spot_near) / (spotLights[i].radius + spot_near - (texture(spotLights[i].shadowTex, vl.xy).r * 2.0f - 1.0f) * (spotLights[i].radius - spot_near)))
            {
                float dist = distance(spotLights[i].position, _v);
    
                curLightCoef += max(0.0f, dot(lightDirection, _vn));
                curLightCoef += pow(max(0.0f, dot(_vn, normalize(lightDirection + normalize(camPos - _v)))), 128f);
                curLightCoef *= pow(spotLights[i].brightness * (spotLights[i].radius - dist) / spotLights[i].radius, (4.0f - 3.0f * spotLights[i].intensity) / 2.0f);
            }
        }
        
        lightColor += spotLights[i].color * curLightCoef; 
    }
    for (int i = 0; i < pointLightsCount; i++)
    {
        curLightCoef = 0f;
        
        vec3 lightVec = _v - pointLights[i].position;
        vec3 lightDirection = -normalize(lightVec);
        float dist = length(lightVec);
        if (dist - max(SHADOW_BIAS_MAX * (1.0f - dot(_vn, -normalize(lightVec))), SHADOW_BIAS_MIN) <= 
            texture(pointLights[i].shadowCube, lightVec).r * pointLights[i].radius)
        {
            curLightCoef += max(0.0f, dot(lightDirection, _vn));
            curLightCoef += pow(max(0.0f, dot(_vn, normalize(lightDirection + normalize(camPos - _v)))), 128f);
            curLightCoef *= pow(pointLights[i].brightness * (pointLights[i].radius - dist) / pointLights[i].radius, (4.0f - 3.0f * pointLights[i].intensity) / 2.0f);
        }

        lightColor += pointLights[i].color * curLightCoef;
    }
    //for (int i = 0; i < lightsCount; i++)
    //{
    //    curLightCoef = 0.0f;
    //
    //    if (lights[i].position.w == 0.0f)
    //    {
    //        if (lights[i].direction.w == 0.0f) // ambient light
    //            curLightCoef = lights[i].coeffs.y;
    //        else                             // directional light
    //        {
    //            vec3 vl = (_vl[i].xyz / _vl[i].w) * 0.5f + 0.5f;
    //            vec3 lightDirection = -lights[i].direction.xyz;
    //            if (vl.z - max(SHADOW_BIAS_MAX * (1.0 - dot(_vn, lightDirection)), SHADOW_BIAS_MIN) < texture(lights[i].shadowTex, vl.xy).r)
    //            {
    //                curLightCoef += max(0.0f, dot(lightDirection, _vn));
    //                curLightCoef += pow(max(0.0f, dot(_vn, normalize(lightDirection + normalize(camPos - _v)))), 128f);
    //                curLightCoef *= lights[i].coeffs.y;
    //            }
    //        }
    //    }
    //    else
    //    {
    //        float dist = distance(lights[i].position.xyz, _v);
    //        if (dist > lights[i].coeffs.x)
    //            continue;
    //        if (lights[i].direction.w == 0.0f) // point light
    //        {
    //            vec3 lightDirection = normalize(lights[i].position.xyz - _v);
    //            curLightCoef += max(0.0f, dot(lightDirection, _vn));
    //            curLightCoef += pow(max(0.0f, dot(_vn, normalize(lightDirection + normalize(camPos - _v)))), 128f);
    //            //curLightCoef *= texture(lights[i].shadowCube, _v - lights[i].position.xyz).r;
    //        }
    //        else                             // spot light
    //        {
    //            vec3 lightDirection = normalize(lights[i].position.xyz - _v);
    //            if (dot(lightDirection, -lights[i].direction.xyz) >= cos(lights[i].coeffs.w / 2.0f))
    //            {
    //                vec3 vl = (_vl[i].xyz / _vl[i].w) * 0.5f + 0.5f;
    //                if ((2.0 * spot_near) / (lights[i].coeffs.x + spot_near - (vl.z * 2.0f - 1.0f) * (lights[i].coeffs.x - spot_near)) - 
    //                    max(SHADOW_BIAS_MAX * (1.0 - dot(_vn, lightDirection)), SHADOW_BIAS_MIN) < 
    //                    (2.0 * spot_near) / (lights[i].coeffs.x + spot_near - (texture(lights[i].shadowTex, vl.xy).r * 2.0f - 1.0f) * (lights[i].coeffs.x - spot_near)))
    //                {
    //                    curLightCoef += max(0.0f, dot(lightDirection, _vn));
    //                    curLightCoef += pow(max(0.0f, dot(_vn, normalize(lightDirection + normalize(camPos - _v)))), 128f);
    //                }
    //            }
    //        }
    //        curLightCoef *= lights[i].coeffs.y * pow((lights[i].coeffs.x - dist) / lights[i].coeffs.x, (4.0f - 3.0f * lights[i].coeffs.z) / 2.0f);
    //    }
    //
    //    lightColor += lights[i].color * curLightCoef;
    //}
    vec3 baseColor = texture(tex, _vt).rgb;
    outColor = vec4(baseColor * lightColor, 1.0f);
}