Shader "Custom/SDFVolume" {
    Properties {
        _Volume ("Volume", 3D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader {
        Tags { "Queue"="Transparent" }
        Cull Back ZWrite Off ZTest Always

        Pass {
            CGPROGRAM
            #pragma vertex vert_sdf
            #pragma fragment frag_sdf
            #include "SDFVolume.cginc"
            ENDCG
        }
    }
}
