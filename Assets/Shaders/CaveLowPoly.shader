Shader "Custom/CaveLowPoly" {
    Properties {
        _LightColor ("LightColor", Color) = (1,1,1,1)
        _DarkColor ("DarkColor", Color) = (1,1,1,1)
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

        fixed4 _LightColor, _DarkColor; 
        
        void surf (Input i, inout SurfaceOutputStandard o) {
            float3 up = float3(0, 1, 0);
            float angle = dot(i.worldNormal, up);
            float4 col;
            if (abs(angle - 1) < 0.2) {
                col = float4(0.5, cos(0.05 * i.worldPos.y), sin(0.05 * i.worldPos.y), 1);
            }
            else {
                angle = (angle + 1) / 2; // put in [0, 1]
                col = (angle * _LightColor + (1 - angle) * _DarkColor) / 2;
            }
            o.Albedo = col.rgb;
            o.Alpha = col.a;
        }
        ENDCG
    }
    Fallback "Diffuse"
  }