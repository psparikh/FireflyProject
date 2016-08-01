Shader "Hidden/uSky GlobalFog" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "black" {}
}

CGINCLUDE

	#include "UnityCG.cginc"

	uniform sampler2D 		_MainTex;
	uniform sampler2D_float	_CameraDepthTexture;
	
	// x = start distance
	uniform float4			_DistanceParams;
	
	int4					_SceneFogMode; // x = fog mode, y = use radial flag
	float4					_SceneFogParams;

	uniform float4			_MainTex_TexelSize;
	
	// for fast world space reconstruction
	uniform float4x4		_FrustumCornersWS;
	uniform float4			_CameraWS;
	
	// x = start distance, y = _ColorDecay, z = Scatter Occlusion
	uniform half3 			_fParams;
	
	uniform half2			_colorCorrection;
	uniform half3			_betaR, _betaM, _miePhase_g, _mieConst; 
	uniform half4			_NightZenithColor;
	uniform float3			_SunDir;		
	uniform half3 			_SkyMultiplier;	
	uniform half4			_NightHorizonColor;
	
	struct v2f {
		float4 pos				: SV_POSITION;
		float2 uv				: TEXCOORD0;
		float2 uv_depth			: TEXCOORD1;
		float4 interpolatedRay	: TEXCOORD2;
	};
	
	v2f vert (appdata_img v)
	{
		v2f o;
		half index = v.vertex.z;
		v.vertex.z = 0.1;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord.xy;
		o.uv_depth = v.texcoord.xy;
		
		#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1-o.uv.y;
		#endif				
		
		o.interpolatedRay = _FrustumCornersWS[(int)index];
		o.interpolatedRay.w = index;
		
		return o;
	}
	
	// Applies one of standard fog formulas, given fog coordinate (i.e. distance)
	half ComputeFogFactor (float coord)
	{
		float fogFac = 0.0;
		if (_SceneFogMode.x == 1) // linear
		{
			// factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
			fogFac = coord * _SceneFogParams.z + _SceneFogParams.w;
		}
		if (_SceneFogMode.x == 2) // exp
		{
			// factor = exp(-density*z)
			fogFac = _SceneFogParams.y * coord; fogFac = exp2(-fogFac);
		}
		if (_SceneFogMode.x == 3) // exp2
		{
			// factor = exp(-(density*z)^2)
			fogFac = _SceneFogParams.x * coord; fogFac = exp2(-fogFac*fogFac);
		}
		return saturate(fogFac);
	}

	// Distance-based fog
	float ComputeDistance (float3 camDir, float zdepth)
	{
		float dist; 
		if (_SceneFogMode.y == 1)
			dist = length(camDir);
		else
			dist = zdepth * _ProjectionParams.z;
		// Built-in fog starts at near plane, so match that by
		// subtracting the near value. Not a perfect approximation
		// if near plane is very large, but good enough.
		dist -= _ProjectionParams.y;
		return dist;
	}

	half4 ComputeFog (v2f i) : SV_Target
	{
		half4 sceneColor = tex2D(_MainTex, i.uv);
		
		// Reconstruct world space position & direction
		// towards this screen pixel.
		float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv_depth);
		float dpth = Linear01Depth(rawDepth);
		float4 wsDir = dpth * i.interpolatedRay;
//		float3 wsPos = (_WorldSpaceCameraPos + wsDir);	
		float4 wsPos = _CameraWS + wsDir;
// --------------------------------------------------------------------------------	
		// scattering color
		float3 viewdir = normalize( wsDir );

		float cosTheta = dot( viewdir,_SunDir); 
		half cosine = cosTheta;
		half t = viewdir.y ;

		half zenithAngle = max(_fParams.y , ( t + _fParams.y ) * min( _fParams.y, dpth ));
//		half zenithAngle = max(_fParams.y ,  t);

		half sR = 8.0 / zenithAngle ;
		half sM = 1.2 / zenithAngle ;
		
		// gradient
		half3 gr = _NightZenithColor.xyz * sR;
		gr *= (2 - gr);									
									
		half3 extinction = exp(-( _betaR * sR + _betaM * sM ));
		half3 rayleigh = lerp( extinction * gr, 1 - extinction, _SkyMultiplier.x );
		half3 mie = rayleigh * sM / rayleigh.r * _mieConst.xyz;

		// scattering phase
		half miePhase = _miePhase_g.x * pow( _miePhase_g.y - _miePhase_g.z * cosine, -1.5 );

		half3 inScatter = half3(0,0,0);
		
		// avoid the mie scattering go through the occlusion
//		inScatter += ( rayleigh * 0.75 + mie * miePhase * saturate( dpth - 0.5 ) ) * (( 1.0 + cosine * cosine ) * _SkyMultiplier.y);

		half occ = lerp(saturate( dpth - (1-_fParams.z) ), 1.0, _fParams.z);
		inScatter = ( rayleigh * 0.75 + mie * miePhase * occ) * (( 1.0 + cosine * cosine ) * _SkyMultiplier.y);

	#ifdef NIGHTSKY_ON			
		inScatter += _NightHorizonColor.xyz * gr;
	#endif

		// tonemapping
	#ifndef USKY_HDR_MODE
		inScatter = 1 - exp(-1 * inScatter);
	#endif
 
		// color correction
		inScatter = pow(inScatter * _colorCorrection.x, _colorCorrection.y);

// --------------------------------------------------------------------------------	
		// Compute fog distance
		float g = _fParams.x + ComputeDistance (wsDir, dpth);

		// Compute fog amount
		half fogFac = ComputeFogFactor (max(0.0,g));
		// Do not fog skybox
		if (rawDepth >= 0.999999)
			fogFac = 1.0;
		
		// Lerp between fog color & original scene color
		// by fog amount
		return lerp (half4(inScatter.rgb,1), sceneColor, fogFac);
	}

ENDCG
// --------------------------------------------------------------------------------	
SubShader
{
	ZTest Always Cull Off ZWrite Off Fog { Mode Off }

	Pass
	{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment ComputeFog
		#pragma multi_compile _ NIGHTSKY_ON
		#pragma multi_compile _ USKY_HDR_MODE
		#pragma target 3.0
		ENDCG
	}
}

Fallback off

}
