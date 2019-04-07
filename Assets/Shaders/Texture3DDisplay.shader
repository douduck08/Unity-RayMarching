Shader "Unlit/Texture3DDisplay" {
    Properties {
        _MainTex ("Texture", 3D) = "white" {}
        _Z ("Z", Range(0, 1)) = 0
        [Toggle]  _Alpha ("Alpha", Int) = 0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _ALPHA_ON
            
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float3 uv : TEXCOORD0;
            };

            sampler3D _MainTex;
            float _Z;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = float3(v.uv, _Z);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                fixed4 color = tex3D(_MainTex, i.uv);

                #ifdef _ALPHA_ON
                return step(0.5, color.a);
                #else
                return float4(color.rgb, 1);
                #endif
            }
            ENDCG
        }
    }
}
