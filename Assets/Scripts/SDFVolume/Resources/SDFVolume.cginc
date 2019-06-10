#ifndef SDF_VOLUME_INCLUDE
#define SDF_VOLUME_INCLUDE

#include "UnityCG.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define MAX_RAY_MARCHING_STEP 64
#define MAX_RAY_MARCHING_DISTANCE 2.0
#define MAX_RAY_MARCHING_PRECISION 0.001

sampler3D _Volume;
float4 _Color;
float _Metallic;
float _Smoothness;

float4 sample_volume (float3 pos) {
    float4 d = tex3D(_Volume, pos + 0.5);
    d = d * 2.0 - 1.0;
    return d;
}

float ray_marching (float3 ray_origin, float3 ray_direction, out float3 pos, out float3 normal) {
    float4 df;
    float dis = MAX_RAY_MARCHING_PRECISION * 2.0;
    float ray_distance = 0.0;

    for (int i = 0; i < MAX_RAY_MARCHING_STEP; i++) {
        ray_distance += dis;
        pos = ray_origin + ray_direction * ray_distance;

        df = sample_volume(pos);
        dis = df.w;
        normal = normalize(df.xyz);

        if(dis < MAX_RAY_MARCHING_PRECISION || ray_distance > MAX_RAY_MARCHING_DISTANCE) break;
    }

    float m = ray_distance > MAX_RAY_MARCHING_DISTANCE ? -1.0 : 1.0;
    return m;
}

float get_atten (float3 pos, float3 lightDir) {
    // TODO: shadow
    return 1.0;
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

    float3 pos, normal;
    float4 output = ray_marching(i.rayOrigin, i.rayDirection, pos, normal);
    clip (output);

    float3 lightDir = mul((float3x3)unity_WorldToObject, _WorldSpaceLightPos0.xyz);
    float atten = get_atten (pos, lightDir);

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

#endif // SDF_VOLUME_INCLUDE
