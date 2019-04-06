// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Volumetric SDF" {
    Properties {
        _Volume ("Texture", 3D) = "" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        [Gamma] _Metallic ("Metallic", Range(0, 1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5

        [Toggle] _WorldSpace ("World Space", Int) = 0
    }
    SubShader {
        Tags { "Queue"="Transparent" }
        Cull Back ZWrite Off ZTest Always

        Pass {
            CGPROGRAM
            #pragma vertex vert_sdf
            #pragma fragment frag_sdf
            #pragma multi_compile _ _WORLDSPACE_ON
            
            #ifdef _WORLDSPACE_ON
                #define SDF_WORLD_SPACE
            #endif
            // #define SDF_SHADOW
            #define SDF_SHADOW_LEVEL 0
            #define SDF_VOLUME
            
            #include "DistanceFunctions.cginc"
            // #define SDF sdf_custom
            // float sdf_custom (float3 pos) {
            //     const float cylinder_radius = 0.25;
            //     const float sphere_radius = 0.5;
            //     const float3 box = float3(0.5, 0.05, 0.05);

            //     float d1 = op_Union(sd_Cylinder_x(pos, 0.0, cylinder_radius), sd_Cylinder_y(pos, 0.0, cylinder_radius));
            //     d1 = op_Union(d1, sd_Cylinder_z(pos, 0.0, cylinder_radius));
            //     d1 = op_SmoothSubtraction(sd_Sphere(pos, 0.0, sphere_radius), d1, 0.1);

            //     float d2 = op_SmoothUnion(sd_Box(pos, 0, box), sd_Box(pos, 0, box.yxz), 0.1);
            //     d2 = op_SmoothUnion(d2, sd_Box(pos, 0, box.yzx), 0.1);

            //     float d3 = sd_Plane(pos, float4(0, 1, 0, 1));

            //     return op_Union(op_Union(d1, d2), d3);
            // }
            #include "RayMarching.cginc"
            #include "UnityCG.cginc"
            ENDCG
        }
    }
}
