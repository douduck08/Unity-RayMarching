Shader "Ray Marching SDF" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
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
                return op_Subtraction(df_Sphere(pos, 0.0, 0.49), df_Cylinder(pos, 0.0, 0.3));
            }
            
            #include "RayMarching.cginc"
            #include "UnityCG.cginc"
            ENDCG
        }
    }
}
