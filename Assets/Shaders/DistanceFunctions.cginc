#ifndef DISTANCE_FUNCTIONS_INCLUDE
#define DISTANCE_FUNCTIONS_INCLUDE

// ref: http://www.iquilezles.org/www/articles/distfunctions/distfunctions.htㄤ

// Distance Field functions
float df_Sphere (float3 pos, float3 center, float radius) {
    return length(pos - center) - radius;
}

float df_Box (float3 pos, float3 box) {
    float3 d = abs(pos) - box;
    return length(max(d, 0.0))
         + min(max(d.x, max(d.y, d.z)), 0.0); // remove this line for an only partially signed sdf 
}

float df_Cylinder (float3 pos, float2 c, float radius) {
    return length(pos.xz - c) - radius;
}

// Boolean operation
float op_Union (float d1, float d2) { min(d1, d2); }
float op_Subtraction (float d1, float d2) { return max(d1, -d2); }
float op_Intersection (float d1, float d2) { return max(d1, d2); }

float opSmoothUnion (float d1, float d2, float k) {
    float h = clamp(0.5 + 0.5 * (d2 - d1) / k, 0.0, 1.0 );
    return lerp(d1, d2, h) - k * h * (1.0 - h);
}

float op_SmoothSubtraction (float d1, float d2, float k) {
    float h = clamp(0.5 - 0.5 * (d2 + d1) / k, 0.0, 1.0);
    return lerp(d1, -d2, h) + k * h * (1.0 - h);
}

float op_SmoothIntersection (float d1, float d2, float k) {
    float h = clamp(0.5 - 0.5 * (d2 - d1) / k, 0.0, 1.0);
    return lerp(d1, d2, h) + k * h * (1.0 - h);
}

#endif // DISTANCE_FUNCTIONS_INCLUDE