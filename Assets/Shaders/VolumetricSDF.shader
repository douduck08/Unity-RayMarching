// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Volumetric SDF" {
    Properties {
        _Volume ("Texture", 3D) = "" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        [Gamma] _Metallic ("Metallic", Range(0, 1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
    }
    SubShader {
        Tags { "Queue"="Transparent" }
        Cull Back ZWrite Off ZTest Always

        Pass {
            CGPROGRAM
            #pragma vertex vert_sdf
            #pragma fragment frag_sdf

            #define SDF_SHADOW
            #define SDF_SHADOW_LEVEL 0
            #define SDF_VOLUME
            #include "RayMarching.cginc"
            ENDCG
        }
    }
}
