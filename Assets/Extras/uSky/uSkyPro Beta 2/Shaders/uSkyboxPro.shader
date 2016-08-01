// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "uSkyPro/uSkyboxPro" 
{
	Properties 
	{
		[NoScaleOffset]		_MoonSampler ("Moon",2D) = "black" {}
		[NoScaleOffset]		_OuterSpaceCube("Outer Space Cubemap", Cube) = "black" {}
//							_turbidity ("Turbidity factor", Range (1,10)) = 1
		[Enum(OFF,0,ON,1)]	_uSkySkyboxOcean ("Skybox Ocean", int) = 0
	}
	
	SubShader 
	{
		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"}
		Cull Off ZWrite Off 
	
    	Pass 
    	{	
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "Atmosphere.cginc"
			
			#pragma multi_compile __ USKY_MULTISAMPLE
			#pragma multi_compile __ USKY_HDR_MODE
			#pragma multi_compile __ UNITY_COLORSPACE_GAMMA
			
			#if defined(UNITY_COLORSPACE_GAMMA)
			#define COLOR_2_LINEAR(color) color*(0.4672*color+0.266)
			#define GAMMA_2_OUTPUT(color) color
			#define HDR_OUTPUT(color)  pow(color* 1.265, 0.735)
			#else
			#define COLOR_2_LINEAR(color) color*color
			#define GAMMA_2_OUTPUT(color) color*color
			#define HDR_OUTPUT(color) color* 0.6129
			#endif
			
			UNITY_DECLARE_TEX2D		(_MoonSampler);
			UNITY_DECLARE_TEXCUBE	(_OuterSpaceCube);
//			half4 		_uSkyGroundColor;
			half4		_NightZenithColor,_NightHorizonColor, _MoonInnerCorona, _MoonOuterCorona;
			float4		_MoonDirSize;
			float4x4	_SpaceRotationMatrix;
//			float		_turbidity;

						
			struct v2f 
			{
    			float4  pos				: SV_POSITION;
    			float4	worldPosAndCamY	: TEXCOORD0;
    			float3	MiePhase_g		: TEXCOORD1;
    			float2	moonTC			: TEXCOORD2;
				float3	spaceTC			: TEXCOORD3;
//				float2	uv 				: TEXCOORD4; // debug
			};

			v2f vert(appdata_base v)
			{
    			v2f OUT;
    			OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
    			OUT.worldPosAndCamY.xyz = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
    			OUT.worldPosAndCamY.w = max(_WorldSpaceCameraPos.y*_uSkyAltitudeScale + _uSkyGroundOffset, 0.0);
//    			OUT.worldPosAndCamY.xyz = lerp(float3(0,0,0), OUT.worldPos, 1/_turbidity ); // no affect
//    			OUT.uv = v.texcoord.xy;// debug
    			
				OUT.MiePhase_g = PhaseFunctionG(_uSkyMieG,_uSkyMieScale);
				 
    			// night sky
    			float3 right = normalize( cross( _MoonDirSize.xyz, float3( 0, 0, 1 )));
				float3 up = cross( _MoonDirSize.xyz, right ); 			
				OUT.moonTC.xy = float2( dot( right, v.vertex.xyz), dot( up, v.vertex.xyz) )*_MoonDirSize.w + 0.5;
				OUT.spaceTC = mul((float3x3)_SpaceRotationMatrix, v.vertex.xyz);
			
    			return OUT;
			}
			
			half4 frag(v2f IN) : SV_Target
			{
			    float3 dir = normalize(IN.worldPosAndCamY.xyz);
			    float nu = dot( dir, _SunDirSize.xyz); // sun direction
			    				
				// if the camera height is outside atmospheric precomputed buffer range, it will occur rendering artifacts
//				float3 camera = float3(0.0,max(_WorldSpaceCameraPos.y*_uSkyAltitudeScale + _uSkyGroundOffset, 0.0) ,0.0); // no lower than sealevel

				half3 extinction;
				half3 col = SkyRadiance(float3(0.0, IN.worldPosAndCamY.w, 0.0), dir, IN.MiePhase_g, extinction) ; // inscatter
//----------------------------------------------------------------------------------				
				// night sky
				half3 nightSkyColor = half3(0,0,0);
				half moonMask = 0.0;
				half gr = 1.0;
//			if ( _SunDirSize.y < 0.25 )
			{				
				// add horizontal night sky gradient
				gr = saturate(extinction.z * .25 / _NightHorizonColor.w );
				gr *= 2 - gr;

				nightSkyColor = lerp(_NightHorizonColor.xyz, _NightZenithColor.xyz, gr);
				// add moon and outer space
				half4 moonAlbedo = UNITY_SAMPLE_TEX2D ( _MoonSampler, IN.moonTC.xy );
				moonMask = moonAlbedo.a;
				
				half4 spaceAlbedo = UNITY_SAMPLE_TEXCUBE (_OuterSpaceCube, IN.spaceTC);		
				nightSkyColor += ( moonAlbedo.rgb * _uSkyNightParams.y + spaceAlbedo.rgb * (( max(1-moonMask,gr)) * _uSkyNightParams.z)) * gr ;

				// moon corona
				float moonDir = 1 - dot( dir, _MoonDirSize.xyz);
				half m = moonDir;
				nightSkyColor += _MoonInnerCorona.xyz * (1.0 / (1.05 + m * _MoonInnerCorona.w));
				nightSkyColor += _MoonOuterCorona.xyz * (1.0 / (1.05 + m * _MoonOuterCorona.w));
			}
//----------------------------------------------------------------------------------				
				#ifndef USKY_HDR_MODE
				col += nightSkyColor;
				col = GAMMA_2_OUTPUT(hdr2(col*_uSkyExposure));
//				col = GAMMA_2_OUTPUT(HDRtoRGB(col*_uSkyExposure));
				#else
				col += COLOR_2_LINEAR(nightSkyColor);// TODO : not correct
				col = HDR_OUTPUT(col*_uSkyExposure);
				#endif

				// add sun disc
				float sun = step(0.9999 - _SunDirSize.w * 1e-4, nu) * sign(_LightColor0.w);
				col += (sun * SUN_BRIGHTNESS) * extinction ;

				half alpha = lerp( 1.0, max(1e-3, moonMask+(1-gr)), _uSkyNightParams.x);
				
				return half4(col, alpha);
			}			
			ENDCG
    	}
	}
	Fallback "uSkyPro/Skybox Mobile SM2 (Altitude Sample X1 only)"

}