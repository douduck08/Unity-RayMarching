Shader "Custom/VolumeViewer" {
    Properties {
        _MainTex ("Texture", 3D) = "white" {}
        _Z ("Z", Range(0, 1)) = 0
        [Toggle] _Alpha ("Alpha", Int) = 0
        [Toggle] _Step ("Step", Int) = 0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _ALPHA_ON
            #pragma multi_compile _ _STEP_ON

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

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = float3(v.uv, _Z);
                return o;
            }

            half4 frag (v2f i) : SV_Target {
                half4 color = tex3D(_MainTex, i.uv);

                #ifdef _ALPHA_ON
                    #ifdef _STEP_ON
                        return step(0.5, color.a);
                    #else
                        return color.a;
                    #endif
                #else
                    #ifdef _STEP_ON
                        return half4(step(0.5, color.rgb), 1);
                    #else
                        return half4(color.rgb, 1);
                    #endif
                #endif
            }
            ENDCG
        }
    }
}
