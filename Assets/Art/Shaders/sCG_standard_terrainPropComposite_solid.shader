// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "socialPointCG/sCG_standard_terrainPropComposite_solid"
{
	Properties
	{
		[Header(GLOBAL PROPERTIES)]
		_MetallicFactor("Metallic Factor",Range(0, 1)) = 0
		_GlossMapScale("Smoothness", Range(0, 1)) = 0

		[Header(TEXTURES)]
		_Col1("Diffuse color 1", Color) = (0.62, 0.59, 0.55, 1.0)
		_Col2("Diffuse color 2", Color) = (0.20, 0.14, 0.07, 1.0)
		_MainTex("CompositeMap", 2D) = "white" {}
		_BumpMap("NormalMap", 2D) = "bump" {}
		_BumpScale("NormalMap Scale", Range(0, 3)) = 1.0
		_BumpSmoothness("Snow NM attenuation", Range(0, 1)) = 0.5

		[Header(WORLDCOORDS. GROUND TEXTURE)]
		_GrassTex("GroundMap", 2D) = "white" {}
		_GrassScale("GroundMap scale", float) = 1

		[Header(SNOW COVERAGE)]
		_SnowCol("Snow color", Color) = (1.0, 1.0, 1.0, 1.0)
		_SnowHeight("Snow level", float) = 0.75
		_SnowBlending("Snow blending", Range(0, 2)) = 0.15

		[Header(DETAILMAP PROPERTIES)]
		_DetailAlbedoMap("DetailMap", 2D) = "white" {}
		_DetailAlbedoScale("DetailMap scale", float) = 1
		[Space(20)]
		_DetailContrastG("(G) - Grass intensity", Range(-2, 2)) = 0
		_DetailContrastB("(B) - Rock intensity", Range(-2, 2)) = 0
		_DetailContrastA("(A) - Snow intensity", Range(-2, 2)) = 0
	}
	
	SubShader
	{
		Tags
		{ 
			"RenderType" = "Opaque" 
			//"Queue" = "Transparent" 
			//"RenderType"="Transparent"
		}
		
		LOD 200

		//Blend SrcAlpha OneMinusSrcAlpha 	// Alpha blending

		CGPROGRAM

		/*
		finalcolor:myFinalColor 
				...works only in forward!
		exclude_path:deferred 
				...forces forward rendering for the shader.
		*/

		// Physically based Standard lighting model, and enable shadows on all light types
		// #pragma surface surf Standard fullforwardshadows vertex:vert finalcolor:myFinalColor exclude_path:deferred 
		#pragma surface surf Standard fullforwardshadows vertex:vert //alpha:fade

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		float _MetallicFactor;
		float _GlossMapScale;

		sampler2D _MainTex;
		fixed4 _Col1;
		fixed4 _Col2;
		
		sampler2D _BumpMap;
		float _BumpScale;
		float _BumpSmoothness;

		sampler2D _GrassTex;
		float _GrassScale;

		float4 _SnowCol;
		float _SnowHeight;
		float _SnowBlending;

		sampler2D _DetailAlbedoMap;				
		float _DetailAlbedoScale;

		float _DetailContrastG;
		float _DetailContrastB;
		float _DetailContrastA;
		

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_GrassTex;

			fixed4 color : COLOR;

			float2 uv_DetailAlbedoMasks;

			float3 worldPos; //<--- world map coord			
		};

		/*
		struct appdata_full 
		{
			//float4 vertex : POSITION;
			//float4 tangent : TANGENT;
			//float3 normal : NORMAL;
			fixed4 color : COLOR;
			//float4 texcoord : TEXCOORD0;
			//float4 texcoord1 : TEXCOORD1;
			//half4 texcoord2 : TEXCOORD2;
			//half4 texcoord3 : TEXCOORD3;
			//half4 texcoord4 : TEXCOORD4;
			//half4 texcoord5 : TEXCOORD5;
		};
		*/

		void vert(inout appdata_full v, out Input o)
		{

			UNITY_INITIALIZE_OUTPUT(Input, o);
		}
		
		/*
		struct SurfaceOutputStandard
		{
			fixed3 Albedo;      // base (diffuse or specular) color
			fixed3 Normal;      // tangent space normal, if written
			half3 Emission;
			half Metallic;      // 0=non-metal, 1=metal
			half Smoothness;    // 0=rough, 1=smooth
			half Occlusion;     // occlusion (default 1)
			fixed Alpha;        // alpha for transparencies
		};
		*/

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			// Diffuse
			fixed4 CM = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 DF_grass = tex2D(_GrassTex, IN.worldPos.zx * _GrassScale);

			fixed4 DF = lerp(_Col2, _Col1, CM.r);

			float snowMask = saturate(smoothstep(_SnowHeight-_SnowBlending*0.5,_SnowHeight+_SnowBlending*0.5, saturate(CM.a + CM.r * CM.a)));
			snowMask = snowMask * CM.b;

			DF = lerp(DF, DF_grass, CM.g); 

			DF = lerp(DF, _SnowCol, snowMask);

			fixed4 DF_detail = tex2D(_DetailAlbedoMap, IN.worldPos.zx * _DetailAlbedoScale);	//<--- world map coords

			fixed detail_grass = saturate(lerp(half3(0.5, 0.5, 0.5), DF_detail.g, _DetailContrastG));
			fixed detail_rock  = saturate(lerp(half3(0.5, 0.5, 0.5), DF_detail.b, _DetailContrastB));
			fixed detail_snow  = saturate(lerp(half3(0.5, 0.5, 0.5), DF_detail.a, _DetailContrastA));

			fixed detail = lerp(detail_rock, detail_grass, CM.g);
			detail = lerp(detail, detail_snow, snowMask);

			o.Albedo = fixed4((DF * detail * 2.0).rgb, 1.0);
			o.Alpha = IN.color.r;

			// Normal
			float bumpScale = _BumpScale * (1.0 - snowMask * _BumpSmoothness);
			float3 NM = lerp(fixed3(0.0,0.0,1.0), UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex)), bumpScale).rgb;

			o.Normal = NM;

			o.Metallic = _MetallicFactor;
			o.Smoothness = _GlossMapScale;
		}

		//works only with forward rendering. you need to force it with the "exclude_path:deferred" option on the #pragma
		/*
		void myFinalColor(Input IN, SurfaceOutputStandard o, inout fixed4 color)
		{
			color *= _OverlayColor;
		}
		*/

	ENDCG

	}
	
	FallBack "Diffuse"
}