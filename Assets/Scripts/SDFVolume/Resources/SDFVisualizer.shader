Shader "Hidden/SDFVisualizer" {
    Properties {
        _MainTex("SDF Volume", 3D) = "white" {}
        _Depth("Depth", Range(0, 1)) = 0.5
        _Mode("Mode", float) = 0
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    struct appdata {
        float4 vertex : POSITION;
        float2 texcoord : TEXCOORD;
    };

    struct v2f {
        float4 vertex : SV_POSITION;
        float2 texcoord : TEXCOORD;
    };

    sampler3D _MainTex;
    float _Depth;
    float _Mode;
    
    v2f vert(appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.texcoord = v.texcoord;
        return o;
    }
    
    half4 frag(v2f i) : SV_Target {
        half4 data = tex3D(_MainTex, float3(i.texcoord, _Depth));
        half3 dist = frac(data.a * 20);
        half3 grad = data.rgb * 2 - 1;
        return half4(lerp(dist, grad, _Mode), 1);
    }

    ENDCG

    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}