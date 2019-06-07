#ifndef RAY_MARCHING_INCLUDE
#define RAY_MARCHING_INCLUDE

#include "DistanceFunctions.cginc"

// Distance Field function. it's also called Distance Map or Distance Transform
#ifndef SDF
    #define SDF sdf_default
#endif
#ifndef SDF_NORMAL
    #define SDF_NORMAL get_normal
#endif

float sdf_default (float3 pos) {
    return sd_Sphere(pos, 0.0, 0.49);
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

        float3 normal = SDF_NORMAL(pos);
        if (dot(normal, ray_direction) > 0) break;
    }

    float m = ray_distance > max_distance ? -1.0 : 1.0;
    return float2(ray_distance, m);
}

float3 get_normal (float3 pos) {
    float3 small_step = float3(0.01, 0.0, 0.0);
    float3 normal = float3(
        SDF(pos + small_step.xyy) - SDF(pos - small_step.xyy),
        SDF(pos + small_step.yxy) - SDF(pos - small_step.yxy),
        SDF(pos + small_step.yyx) - SDF(pos - small_step.yyx)
    );
    return normalize(normal);
}

float get_atten_hard (float3 ray_origin, float3 ray_direction) {
    const float precision = 0.01;
    const float max_distance = 100.0;
    float3 pos;
    float dis = precision * 10.0;
    float ray_distance = 0.0;

    float rest = 1.0;
    for (int i = 0; i < 64; i++) {
        if(dis < precision || ray_distance > max_distance) break;
        ray_distance += dis;
        pos = ray_origin + ray_direction * ray_distance;
        dis = SDF(pos);
    }

    float atten = ray_distance > max_distance ? rest : 0.0;
    return atten;
}

float get_atten_soft1 (float3 ray_origin, float3 ray_direction) {
    const float precision = 0.01;
    const float max_distance = 100.0;
    float3 pos;
    float dis = precision * 10.0;
    float ray_distance = 0.0;

    const float k = 8;
    float rest = 1.0;
    for (int i = 0; i < 64; i++) {
        if(dis < precision || ray_distance > max_distance) break;
        ray_distance += dis;
        pos = ray_origin + ray_direction * ray_distance;
        dis = SDF(pos);

        rest = min(rest, k * dis / ray_distance);
    }

    float atten = ray_distance > max_distance ? rest : 0.0;
    return atten;
}

float get_atten_soft2 (float3 ray_origin, float3 ray_direction) {
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

        float y = dis * dis / (2.0 * pre_dis);
        float d = sqrt(dis * dis - y * y);
        rest = min(rest, k * dis / max(0.0, ray_distance - y));
        pre_dis = dis;
    }

    float atten = ray_distance > max_distance ? rest : 0.0;
    return atten;
}

#endif // RAY_MARCHING_INCLUDE