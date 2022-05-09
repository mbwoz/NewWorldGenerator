Shader "Custom/PointSurfaceGPU" {
	Properties {
		_Smoothness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.5
		_Occlusion ("Occlusion", Range(0,1)) = 0.5
		_Phase ("Phase", Range(1.0, 128.0)) = 16.0
		_Color1 ("Color1", color) = (0.73, 0.63, 0.3, 1) // gold1
		_Color2 ("Color1", color) = (0.73, 0.78, 0.8, 1) // silver
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		CGPROGRAM
		#pragma surface configureSurface Standard
		#pragma instancing_options assumeuniformscaling procedural:configureProcedural
		#pragma editor_sync_compilation
		#pragma target 4.5

	
		struct Input {
			float3 worldPos;
		};

		float _Smoothness, _Metallic, _Occlusion, _Phase, _Step;
		float4 _Color1, _Color2;

		#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
			StructuredBuffer<float3> _Positions;
		#endif

		void configureProcedural () {
			#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
				float3 position = _Positions[unity_InstanceID];

				unity_ObjectToWorld = 0.0;
				unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0);
				unity_ObjectToWorld._m00_m11_m22 = _Step;
			#endif
		}
		
		void configureSurface (Input input, inout SurfaceOutputStandard surface) {
			float r = (sin(_Time * 3.14 * _Phase) + 1.0) / 2.0;
			surface.Albedo = (r * _Color1 + (1 - r) * _Color2).rgb;
			surface.Smoothness = _Smoothness;
			surface.Metallic = _Metallic;
			surface.Occlusion = _Occlusion;
		}
		ENDCG
	}
	FallBack "Diffuse"
}