Shader "Custom/CaveLowPoly"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float3 worldPos : TEXCOORD1;
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.normal = v.normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //float4 col_green = float4(0.2, 0.6, 0.2, 1);
                float4 col_bronze = (float4(0.6, 0.4, 0.25, 1) + float4(0.4, 0.4, 0.4, 1)) / 2;
                float4 col_gray = float4(0.1, 0.1, 0.1, 1);
                float3 up = float3(0, 1, 0);
                
                float d = dot(up, i.normal);
                if (abs(d - 1) < 0.1) {
                    float4 wave = float4(0.5, cos(0.05 * i.worldPos.y), sin(0.05 * i.worldPos.y), 1);
                    //float4 shade = float4(i.normal, 1);
                    return wave;
                }
                float4 col = col_bronze * (1 + d)/2 + col_gray * (1 - d)/2;
                return col;
            }
            ENDCG
        }
    }
}
