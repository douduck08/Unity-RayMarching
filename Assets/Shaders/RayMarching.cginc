// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'

#ifndef RAY_MARCHING_INCLUDE
#define RAY_MARCHING_INCLUDE

#include "UnityCG.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"
#include "DistanceFunctions.cginc"

float4x4 _FrustumCorners;
sampler3D _Volume;
float4 _Color;
float _Metallic;
float _Smoothness;

// Distance Field function. it's also called Distance Map or Distance Transform
#ifdef SDF_VOLUME
    // Use Texture3D as Distance Field function.
    #define SDF sdf_volume_distance
#else
    #ifndef SDF
        #define SDF sdf_default
    #endif
#endif

#ifndef SDF_SHADOW_LEVEL
    #define SDF_SHADOW_LEVEL 2
#endif

float sdf_default (float3 pos) {
    return sd_Sphere(pos, 0.0, 0.49);
}

float sdf_volume_distance (float3 pos) {
    return tex3D (_Volume, pos + 0.5).r * 2.0 - 1.0;
}

// Ray marching
float inside_volume(float3 pos) {
    float3 s = step(-0.5, pos) - step(0.5, pos);
    return s.x * s.y * s.z;
}

float2 ray_marching (float3 ray_origin, float3 ray_direction) {
    const float precision = 0.01;
    const float max_distance = 100.0;
    float3 pos;
    float dis = precision * 2.0;
    float ray_distance = 0.0;

    for (int i = 0; i < 64; i++) {
        if(dis < precision || ray_distance > max_distance) break;
        ray_distance += dis;

        pos = ray_origin + ray_direction * ray_distance;
        dis = SDF(pos);
    }

    float m = ray_distance > max_distance ? -1.0 : 1.0;
    return float2(ray_distance, m);
}

float3 get_normal (float3 pos) {
    #ifdef SDF_NORMAL
        return SDF_NORMAL(pos);
    #else
        float3 small_step = float3(0.01, 0.0, 0.0);
        float3 normal = float3(
            SDF(pos + small_step.xyy) - SDF(pos - small_step.xyy),
            SDF(pos + small_step.yxy) - SDF(pos - small_step.yxy),
            SDF(pos + small_step.yyx) - SDF(pos - small_step.yyx)
        );
        return normalize(normal);
    #endif
}

float get_atten (float3 ray_origin, float3 ray_direction) {
    const float precision = 0.01;
    const float max_distance = 100.0;
    float3 pos;
    float dis = precision * 10.0;
    float ray_distance = 0.0;

    const float k = 8;
    float rest = 1.0;
    float pre_dis = 1e20;

    for (int i = 0; i < 64; i++) {
        if(dis < precision || ray_distance > max_distance) break;
        ray_distance += dis;
        pos = ray_origin + ray_direction * ray_distance;
        dis = SDF(pos);

        #if SDF_SHADOW_LEVEL == 2
            float y = dis * dis / (2.0 * pre_dis);
            float d = sqrt(dis * dis - y * y);
            rest = min(rest, k * dis / max(0.0, ray_distance - y));
            pre_dis = dis;
        #elif SDF_SHADOW_LEVEL == 1
            rest = min(rest, k * dis / ray_distance);
        #else
            #error invalidate shadow level
        #endif
    }

    float atten = ray_distance > max_distance ? rest : 0.0;
    return atten;
}

// vertex & fragment shader
struct appdata {
    float4 vertex : POSITION;
};

struct v2f_ray {
    float4 vertex : SV_POSITION;
    float3 rayOrigin : TEXCOORD0;
    float3 rayDirection : TEXCOORD1;
};

v2f_ray vert_sdf (appdata v) {
    v2f_ray o;
    o.vertex = UnityObjectToClipPos(v.vertex);

    float3 cameraObjPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
    o.rayOrigin = v.vertex;
    o.rayDirection = v.vertex - cameraObjPos;

    return o;
}

float4 frag_sdf (v2f_ray i) : SV_Target {
    i.rayDirection = normalize(i.rayDirection);
    float2 output = ray_marching(i.rayOrigin, i.rayDirection);
    clip (output.y);

    float3 pos = i.rayOrigin + i.rayDirection * output.x;
    float3 normal = get_normal(pos);

    float3 lightDir = mul((float3x3)unity_WorldToObject, _WorldSpaceLightPos0.xyz);
#ifdef SDF_SHADOW
    float atten = get_atten (pos, lightDir);
#else
    float atten = 1.0;
#endif

    float3 specColor;
    float oneMinusReflectivity;
    float3 albedo = DiffuseAndSpecularFromMetallic(_Color, _Metallic, /*out*/specColor, /*out*/oneMinusReflectivity);

    UnityLight light;
    UNITY_INITIALIZE_OUTPUT(UnityLight, light);
    light.dir = lightDir;
    light.color = _LightColor0.rgb * atten;
    light.ndotl = saturate(dot(normal, light.dir));

    UnityIndirect indirectLight;
    UNITY_INITIALIZE_OUTPUT(UnityIndirect, indirectLight);
    indirectLight.diffuse += max(0, ShadeSH9(float4(normal, 1)));

    return UNITY_BRDF_PBS(albedo, specColor, oneMinusReflectivity, _Smoothness, normal, -i.rayDirection, light, indirectLight);
}

#endif // RAY_MARCHING_INCLUDE