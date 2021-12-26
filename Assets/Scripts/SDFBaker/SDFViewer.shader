Shader "Hidden/SDF Viewer" {
    Properties {
        _SDFTexture("SDF Volume", 3D) = "white" {}
        _Slice("Depth", Range(0, 1)) = 0.5
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    struct appdata {
        float4 vertex : POSITION;
        float2 texcoord : TEXCOORD;
    };

    struct v2f {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD;
    };

    sampler3D _SDFTexture;
    float _Slice;
    
    v2f vert(appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.texcoord;
        return o;
    }
    
    half4 frag(v2f i) : SV_Target {
        float raw = tex3D(_SDFTexture, float3(i.uv, _Slice)).r;
        return half4(raw > 0.51, 0, 0, 1);

        float dis = raw * 2.0 - 1.0;
        float positive = frac(max(dis, 0) * 10);
        float negtaive = frac(max(-dis, 0) * 10);
        return half4(positive, 0, negtaive, 1);
    }

    ENDCG

    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            Cull off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}