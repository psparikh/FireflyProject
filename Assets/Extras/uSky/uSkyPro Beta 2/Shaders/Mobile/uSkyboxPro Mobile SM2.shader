// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// --------------------------------------------------------------------
// This shader designed for Moblie platform or Fallback shader (SM 2.0)
// --------------------------------------------------------------------
// LIMITATION :
//
// Supports only Altitude Sample X1 
// No calculation of camera position 
// No Altitude Scale
// Always at ground/sea level camera view from the sky
// Always disabled skybox ocean effect
// No earth shadow
// No Moon Corona
//
// Ported Unity Photographic tonemapping formular for skybox :
// - Sky color is more saturated then original color
// - Slightly darker on Zenith (top of the sky)
// ---------------------------------------------------------
// Tested on original iPhone 5 (NOT 5c or 5s)
// Build: lL2cpp iOS 9, with "Fast but no Exception" setting	  
// Skybox only (full day/night cycle):	60fps  GPU: 16ms
// ---------------------------------------------------------
// Stats for Vertex shader:		75 math
// Stats for Fragment shader:	50 avg math (48..52), 4 texture
 
Shader "uSkyPro/Skybox Mobile SM2 (Altitude Sample X1 only)" 
{
	Properties {
		[NoScaleOffset] _MoonSampler ("Moon",2D) = "black" {}
		[NoScaleOffset]	_OuterSpaceCube("Outer Space Cubemap", Cube) = "black" {}
		[HideInInspector]_turbidity ("Turbidity factor", Range (1,10)) = 1 // temp hide
	}
	SubShader 
	{
	Pass{	Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"}
			ZWrite Off Cull Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "Atmosphere_Mobile.cginc"
			
			#pragma multi_compile __ USKY_HDR_MODE
			#pragma multi_compile __ UNITY_COLORSPACE_GAMMA
			
		#if defined(UNITY_COLORSPACE_GAMMA)
			#define LDR_OUTPUT(color) color
			#define HDR_OUTPUT(color) pow(color* 1.265, 0.735)
		#else
			#define LDR_OUTPUT(color) color*color
			#define HDR_OUTPUT(color) color* 0.6129
		#endif
		
			float _turbidity;
			
			struct v2f 
			{
    			float4	pos				: SV_POSITION;
    			float3	worldPos		: TEXCOORD0;
    			float2	Mu_uMuS			: TEXCOORD1;
    			half3	MiePhase_g		: TEXCOORD2;
    			half2	NightGradient	: TEXCOORD3;
    			half3	NightSkyColor	: TEXCOORD4;
    			half2	moonTC			: TEXCOORD5;
    			float3	spaceTC			: TEXCOORD6;   			
			};
						
			v2f vert(appdata_base v)
			{
    			v2f OUT;
    			OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
    			OUT.worldPos = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
    			
    			float zenith = normalize(OUT.worldPos).y;
    			// approximation of optical depth
    			float opticalDepth = lerp(0, zenith * (UNITY_PI * 2), 1/_turbidity)+ _uSkyGroundOffset / 2e4;
    			OUT.Mu_uMuS.x = max(0.08, opticalDepth); // 0.08 to fix the ground rendering issue for mobile build

   				float uMuS = Get_uMuS (_SunDirSize.y);
   				// read the second table only from right side of the precomputed inscatter texture
   				// no earth shadow
				OUT.Mu_uMuS.y = (uMuS + float(RES_NU) - 2) / float(RES_NU) ; 
				
    			// horizontal night sky gradient
    			float gr = saturate(opticalDepth * 0.1 / _NightHorizonColor.w);
    			gr *= 2 - gr;
    			OUT.NightGradient.x = gr * _uSkyNightParams.y;
    			OUT.NightGradient.y = max(1e-3, 1 - gr);
    			
    			OUT.NightSkyColor = lerp(_NightHorizonColor.xyz, _NightZenithColor.xyz, gr);
    			
    			// mie G term
				OUT.MiePhase_g = PhaseFunctionG(_uSkyMieG, _uSkyMieScale);

    			// night sky
    			float3 right = normalize( cross( _MoonDirSize.xyz, float3( 0, 0, 1 )));
				float3 up = cross( _MoonDirSize.xyz, right ); 			
				OUT.moonTC.xy = float2( dot( right, v.vertex.xyz), dot( up, v.vertex.xyz) )*_MoonDirSize.w + 0.5;
				OUT.spaceTC = mul((float3x3)_SpaceRotationMatrix, v.vertex.xyz);
    			return OUT;
			}
			

			half4 frag(v2f IN) : SV_Target
			{
			    float3 skyDir = normalize(IN.worldPos);
			    float nu = dot( skyDir, _SunDirSize.xyz); // sun direction

				// Use medium precision
				half3 extinction = half3(1,1,1);
			    half3 col = Texture2D_Mobile_2 (skyDir.y, IN.Mu_uMuS, nu, IN.MiePhase_g, extinction); // inScatter
				
//------------------------------------------------------------------------------------------------------
				// night sky
				col += IN.NightSkyColor;
				
				// optional night sky elements
				// add moon
				half4 moonAlbedo = UNITY_SAMPLE_TEX2D ( _MoonSampler, IN.moonTC.xy);
//				col += moonAlbedo.rgb * IN.NightGradient.x;
				
				// add outer space and dithering
				half3 spaceAlbedo = UNITY_SAMPLE_TEXCUBE (_OuterSpaceCube, IN.spaceTC).rgb;
				spaceAlbedo *= _uSkyNightParams.z * ( 1 - moonAlbedo.a);
				
				col += ( moonAlbedo.rgb + spaceAlbedo.rgb) * IN.NightGradient.x;
//------------------------------------------------------------------------------------------------------

			#ifndef USKY_HDR_MODE
				col = HDRtoLDR(col);
				col = LDR_OUTPUT(col);
			#else
				col = col * _uSkyExposure.x;
				col = HDR_OUTPUT(col);
			#endif

				// add sun disk
				half sun = step(0.9999 - _SunDirSize.w * 1e-4, nu);
				col += (sun * SUN_BRIGHTNESS * sign(_LightColor0.w)) * extinction;

				half alpha = lerp(1.0, moonAlbedo.a + IN.NightGradient.y, _uSkyNightParams.x);
																								
				return half4(col, alpha);	
				
			}
			ENDCG
    	}
	}
}