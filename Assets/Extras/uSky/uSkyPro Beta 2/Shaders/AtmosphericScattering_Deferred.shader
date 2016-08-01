Shader "Hidden/uSkyPro AtmosphericScattering Deferred"
{
	Properties 
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_uSkySkyboxOcean ("",Float) = 0
	}

	CGINCLUDE

	#pragma target 3.0
	#pragma exclude_renderers gles gles3 metal
	
	#include "UnityCG.cginc"
	#include "Atmosphere.cginc"
	#include "uSkyPostEffect.cginc"

	#if defined(UNITY_COLORSPACE_GAMMA)
	#define COLOR_2_LINEAR(color) color*(0.4672*color+0.266)*2.2
	#define GAMMA_2_OUTPUT(color) color
	#define HDR_OUTPUT(color) color
	#else
	#define COLOR_2_LINEAR(color) color*color
	#define GAMMA_2_OUTPUT(color) color*color
	#define HDR_OUTPUT(color) color* 0.6129
	#endif
				
	UNITY_DECLARE_TEX2D ( _MainTex );
	UNITY_DECLARE_TEX2D (u_OcclusionTexture);

	uniform float4		_MainTex_TexelSize;
	uniform float		_ScatteringIntensity,_ScatterExtinction;
	uniform float4		_NightZenithColor, _NightHorizonColorDeferred;

	// x = OcclusionDarkness, y = OcclusionSky (0.f),  z = Use Occlusion (On/Off)
	uniform float3		_OcclusionDarkness;

	struct v2f 
	{
		float4 pos				: SV_POSITION;
		float2 uv				: TEXCOORD0;
		float2 uv_depth			: TEXCOORD1;
		float3 interpolatedRay 	: TEXCOORD2;
		float3 MiePhase_g		: TEXCOORD3;
	};
	
	v2f vert( appdata_img v )
	{
		v2f o;
		half index = v.vertex.z;
		v.vertex.z = 0.1;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.pos.w = 1;
		o.uv = v.texcoord.xy;
		o.uv_depth = v.texcoord.xy;
		
		#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1-o.uv.y;
		#endif				
														
		o.interpolatedRay = FrustumCorners((int)index);
		
		o.MiePhase_g = PhaseFunctionG(_uSkyMieG,_uSkyMieScale);
		
		return o;
	}

	half2 UVFromPos(half2 pos) {
	#if defined(UNITY_PASS_FORWARDBASE)
		return pos;
	#else
		return pos / _ScreenParams.xy;
	#endif
	}

	half4 frag( v2f i, 
#if defined(SHADER_API_D3D11) || defined(SHADER_API_GLCORE)
		UNITY_VPOS_TYPE vpos : SV_Position
#else
		UNITY_VPOS_TYPE vpos : VPOS // glcore error in Unity 5.3
#endif
	) : SV_Target 
	{
		half4 sceneColor = UNITY_SAMPLE_TEX2D (_MainTex, i.uv);

		float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv_depth);
		float depth = Linear01Depth(rawDepth);
		// only calculate the camera height value
		float3 camera = float3(0.0, _WorldSpaceCameraPos.y ,0.0); 
		float3 worldPos = camera + depth * i.interpolatedRay.xyz;

		half3 extinction = half3(0,0,0);
		half3 inscatter = InScattering(camera, worldPos, i.MiePhase_g, extinction);

		float occlusionMask = 1.0;

	/* The BlackSmith Occlusion	*/
	if (_OcclusionDarkness.z > 0.0)
	{	
		// if use the i.uv, occasionally we got the uv flipped upside down on d3d11, 
		// so we use orignal approach from BlackSmith method by using UVFromPos().
		float2 uv_occ = UVFromPos(vpos.xy);
		float occTex = UNITY_SAMPLE_TEX2D (u_OcclusionTexture, uv_occ).r;
		float occBias = _OcclusionDarkness.x;
		occlusionMask = lerp (occTex, 1.0, occBias);
		inscatter *= occlusionMask * occlusionMask;
	}
		
		// Do not fog skybox
		if(rawDepth > 0.9999999f) 
			return sceneColor;

		half3 col = sceneColor.rgb * extinction + inscatter;
		
		// extra scattering controls
		col = lerp(sceneColor.rgb, col,_ScatteringIntensity); 	
		col = lerp(col, inscatter*_ScatteringIntensity, _ScatterExtinction);

		// night gradient
		half gr = saturate(extinction.z * 0.25 / _NightHorizonColorDeferred.w);
		half3 nightSkyColor = lerp(_NightHorizonColorDeferred.xyz, _NightZenithColor.xyz, gr);								
		
		#ifndef USKY_HDR_MODE
		col += nightSkyColor;
		col = GAMMA_2_OUTPUT(hdr2(col*_uSkyExposure));
		#else
		col += COLOR_2_LINEAR(nightSkyColor);
		col = HDR_OUTPUT(col*_uSkyExposure);
		#endif
		
		return half4(col, 1.0);

//	return occlusionMask.rrrr; // debug occlusion
//	return half4(inscatter,1); // debug inscattering color
								
	}

	half4 frag_occlusion( 
#if defined(SHADER_API_D3D11) || defined(SHADER_API_GLCORE)
		UNITY_VPOS_TYPE vpos : SV_Position
#else
		UNITY_VPOS_TYPE vpos : VPOS // glcore error in Unity 5.3
#endif
	) : SV_Target 
	{			
		float2 uv_occ = UVFromPos(vpos.xy);
		float occTex = UNITY_SAMPLE_TEX2D (u_OcclusionTexture, uv_occ).r; 
		float occlusionMask = lerp (occTex, 1.0, _OcclusionDarkness);

		return half4 (occlusionMask.rrr,1);
	}
ENDCG

SubShader {
	ZTest Always Cull Off ZWrite Off

	// #0 default composition
	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		#pragma multi_compile __ USKY_MULTISAMPLE
		#pragma multi_compile __ USKY_HDR_MODE
		#pragma multi_compile __ UNITY_COLORSPACE_GAMMA

		ENDCG
	}

	// #1 debug occlusion
    Pass {
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_occlusion
		ENDCG
	}
}
Fallback off
}
