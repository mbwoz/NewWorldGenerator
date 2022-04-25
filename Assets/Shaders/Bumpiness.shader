Shader "Custom/Bumped"
{
    Properties
    {
        _Color ("WallTint", Color) = (1,1,1,1)
        _MainTex ("WallTexture", 2D) = "white" {}
        _FloorTex ("FloorTexture", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types

        #pragma vertex vert
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex, _FloorTex;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_FloorTex;
            float3 worldNormal;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata_full v)
        {
            float4 splat = tex2Dlod(_MainTex, v.texcoord);
            float h = splat.r;
            if (distance(normalize(v.normal), float3(0, 1, 0)) >= 0.1f)
                v.vertex.xyz += v.normal * h * 0.2 * _SinTime.w;
        }
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c;
            if (distance(normalize(IN.worldNormal), float3(0, 1, 0)) < 0.1f)
                c = tex2D (_FloorTex, IN.uv_FloorTex) * float4(0.5, cos(0.05 * IN.worldPos.y), sin(0.05 * IN.worldPos.y), 1);
            else
                c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
