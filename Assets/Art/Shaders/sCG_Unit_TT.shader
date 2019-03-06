Shader "DragonSlides/Unit_TT"
{
	Properties 
	{
		[HideInInspector]
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		[PowerSlider(5.0)] _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf BlinnPhong
		#pragma target 2.0

		sampler2D _MainTex;
		fixed4 _Color;
		half _Shininess;

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o) 
		{
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = (tex.a * _Color) + (tex.rgb - tex.a);
			o.Gloss = tex.a;
			o.Alpha = tex.a * _Color.a;
			o.Specular = _Shininess;
		}

		ENDCG
	}

	FallBack "Legacy Shaders/Specular"
}
