Shader "Custom/GradientSurf" {
    Properties {
        _Phase ("Phase", float) = 0.08
        _Offset ("Offset", float) = 0.0
        _BlueIntensity ("BlueIntensity", float) = 0.8
        _ShadingIntensity ("ShadingIntensity", float) = 0.25
    }
    SubShader {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        struct Input {
            float4 color : COLOR;
            float3 worldNormal;
            float3 worldPos;
        };

        float _Phase, _Offset, _BlueIntensity, _ShadingIntensity;

        void surf (Input i, inout SurfaceOutputStandard o) {
            float4 wave = float4(sin(_Phase * i.worldPos.y + _Offset), cos(_Phase * i.worldPos.y + _Offset), _BlueIntensity, 1);
            float4 shade = float4(i.worldNormal, 1);
            float4 col = (1 - _ShadingIntensity) * wave + (_ShadingIntensity) * shade;
            o.Albedo = col.rgb;
            o.Alpha = col.a;
        }
        ENDCG
    }
    Fallback "Diffuse"
  }