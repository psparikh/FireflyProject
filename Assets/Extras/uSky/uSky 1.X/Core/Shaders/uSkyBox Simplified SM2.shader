// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader Model 2.0 shader
// No mie Scattering
// No moon
// No Outer Space Cubemap

Shader "uSky/uSkyBox Simplified SM2" 
{
Properties {

//[HideInInspector]	_SkyMultiplier("_SkyMultiplier", Vector) = (1, 4, 0.15, 0)
//[HideInInspector]	_betaR("BetaR", Vector) = (0.0058, 0.0136, 0.0331, 0)
//[HideInInspector]	_betaM("BetaM", Vector) = (0.004, 0.005, 0.006, 0)
[HideInInspector]	_GroundColor ("Ground Color", Vector) = (0.2, 0.6, 1.4, 0)
//[HideInInspector]	_SunDir ("Sun Direction", Vector) = (0.321,0.766,-0.557,0)

[HideInInspector]	_NightHorizonColor("_Night Horizon Color", Vector) = (0.43, 0.47, 0.5, 1)
[HideInInspector]	_colorCorrection ("Color Correction", Vector) = (1,1,0,0)

}
SubShader 
	{
		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"}
		Cull Off ZWrite Off  

		CGINCLUDE
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		
		struct appdata_t {
			float4 vertex 			: POSITION;
		};
		
		struct v2f 
		{
			float4	pos 			: SV_POSITION;
			float3	worldPos 		: TEXCOORD0;
			half4	zenithAngle		: TEXCOORD1;
		};
		
		uniform half 		_SunSize;
		uniform half2		_colorCorrection;
		uniform half3		_betaR, _betaM, _miePhase_g, _mieConst; 
		uniform half4		_NightZenithColor,_GroundColor;
		uniform float3		_SunDir;
		uniform float4x4 	rotationMatrix;
		
		// x = Sunset, y = Day, z = Night 
		uniform half4		_SkyMultiplier;
		#ifdef NIGHTSKY_ON
		uniform half4		_NightHorizonColor;
		#endif	
		v2f vert(appdata_t v)
		{
			v2f OUT;
			OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			OUT.worldPos = normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));

			float t = OUT.worldPos.y;
			OUT.zenithAngle.xyz = max(6e-2, t + 6e-2) + (max(0.0, -t ) * _GroundColor.xyz);
			OUT.zenithAngle.w = max(0, t);
			
			return OUT;
		}
		
		half4 frag(v2f IN) : SV_Target
		{
		    float3 pos = normalize( IN.worldPos );
			float cosTheta = dot( pos, _SunDir); // need precision float in iOS
			half cosine = cosTheta;
			
			// optical depth
			half3 sR = 8.0 / IN.zenithAngle.xyz ;
			half3 sM = 1.2 / IN.zenithAngle.xyz ;
			
			// gradient
			half3 gr = _NightZenithColor.xyz * sR;
			gr *= (2 - gr);
			
			// sky color
			half3 extinction = exp(-( _betaR * sR + _betaM * sM ));
			
			half3 rayleigh = lerp( extinction * gr, 1 - extinction, _SkyMultiplier.x );
			half3 mie = rayleigh * sM / rayleigh.r * _mieConst * sign(_LightColor0.w);

			// scattering phase (rayleigh only)
			half3 inScatter = (rayleigh * 0.75 + mie) * (( 1.0 + cosine * cosine )* _SkyMultiplier.y);
			
			// add sun
			half sun = min(1e3, pow((1-cosine)* _SunSize, -1.5 ));
			inScatter += sun * min(mie,IN.zenithAngle.w)* extinction ;

// --------------------------------------------------------------------------------				
			#ifdef NIGHTSKY_ON
				// night horizontal gradient
				inScatter += _NightHorizonColor.xyz * gr;			
			#endif
// --------------------------------------------------------------------------------		

			// tonemapping
			#ifndef USKY_HDR_MODE
			inScatter = 1 - exp(-1 * inScatter);
			#endif
			
			// color correction
			inScatter = pow(inScatter * _colorCorrection.x, _colorCorrection.y);
			
			// "max 1e-3" to avoid the real time reflection probe render with black issue
			half alpha = lerp( 1e-3, 1.0, _SkyMultiplier.x);
			
			return half4(inScatter, alpha); 

		}

		ENDCG
// --------------------------------------------------------------------------------			
	Pass{	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ NIGHTSKY_ON 
			#pragma multi_compile _ USKY_HDR_MODE 
			ENDCG
    	}
	}
	
	Fallback "Skybox/Procedural"
}