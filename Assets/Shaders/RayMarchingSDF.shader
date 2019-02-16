Shader "Ray Marching SDF" {
    Properties {
        _Color ("Color", Color) = (1, 1, 1, 1)
        [Gamma] _Metallic ("Metallic", Range(0, 1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
    }
    SubShader {
        Tags { "RenderType"="Opaque" }

        Pass {
            CGPROGRAM
            #pragma vertex vert_rayMarching
            #pragma fragment frag_rayMarching

            #include "DistanceFunctions.cginc"
            #define SDF sdf_custom
            float sdf_custom (float3 pos) {
                const float cylinder_radius = 0.25;
                const float sphere_radius = 0.5;
                const float box = 0.05;

                float d1 = op_Union(df_Cylinder_x(pos, 0.0, cylinder_radius), df_Cylinder_y(pos, 0.0, cylinder_radius));
                d1 = op_Union(d1, df_Cylinder_z(pos, 0.0, cylinder_radius));
                d1 = op_SmoothSubtraction(df_Sphere(pos, 0.0, sphere_radius), d1, 0.1);

                float d2 = op_SmoothUnion(df_Box(pos, float3(0.5, box, box)), df_Box(pos, float3(box, 0.5, box)), 0.1);
                d2 = op_SmoothUnion(d2, df_Box(pos, float3(box, box, 0.5)), 0.1);

                return op_Union(d1, d2);
            }
            
            #include "RayMarching.cginc"
            #include "UnityCG.cginc"
            ENDCG
        }
    }
}
