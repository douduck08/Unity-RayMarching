#ifndef RAY_MARCHING_INCLUDE
#define RAY_MARCHING_INCLUDE

#include "UnityCG.cginc"
#include "DistanceFunctions.cginc"

// Distance Field function. it's also called Distance Map or Distance Transform 
#ifndef SDF
#define SDF sdf_default
#endif

float sdf_default (float3 pos) {
    return df_Sphere(pos, 0.0, 0.49);
}

// Ray marching
float2 ray_marching (float3 ray_origin, float3 ray_direction) {
    const float precision = 0.0001;
    float max_distance = 10.0;
    float h = precision * 2.0;
    float t = 0.0;
    float m = 1.0;
    
    for (int i = 0; i < 75; i++) {
        if(h < precision || t > max_distance) break;
        t += h;
        h = SDF(ray_origin + ray_direction * t);
    }
    if(t > max_distance) m = -1.0;
    return float2(t, m);
}

float3 get_normal (float3 pos) {
    float3 small_step = float3(0.0001, 0.0, 0.0);
    float3 normal = float3(
        SDF(pos + small_step.xyy) - SDF(pos - small_step.xyy),
        SDF(pos + small_step.yxy) - SDF(pos - small_step.yxy),
        SDF(pos + small_step.yyx) - SDF(pos - small_step.yyx)
    );
    return normalize(normal);
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

v2f_ray vert_rayMarching (appdata v) {
    v2f_ray o;
    o.vertex = UnityObjectToClipPos(v.vertex);

    float3 cameraObjPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
    o.rayOrigin = v.vertex;
    o.rayDirection = v.vertex - cameraObjPos;

    return o;
}

float4 frag_rayMarching (v2f_ray i) : SV_Target {
    i.rayDirection = normalize(i.rayDirection);
    float2 output = ray_marching(i.rayOrigin, i.rayDirection);
    clip (output.y);

    float3 pos = i.rayOrigin + i.rayDirection * output.x;
    float3 normal = get_normal(pos);
    return float4 (normal * 0.5 + 0.5, 1);
}

#endif // RAY_MARCHING_INCLUDE