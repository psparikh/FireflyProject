// Upgrade NOTE: replaced 'unity_World2Shadow' with 'unity_WorldToShadow'

/*=============================================================================
	Original from : 
	The Blacksmith: Atmospheric Scattering Demo Project
	https://www.assetstore.unity3d.com/en/#!/content/39939
	
	Modified by Michael Lam 2015
=============================================================================*/

Shader "Hidden/AtmosphericScattering_Occlusion" {

	CGINCLUDE
	#pragma target 3.0
//	#pragma only_renderers d3d11 d3d9 opengl glcore
	#pragma exclude_renderers gles gles3 metal

	/* this forces the HW PCF path required for correctly sampling the cascaded shadow map
	   render texture (a fix is scheduled for 5.2) */
	#pragma multi_compile SHADOWS_NATIVE

	#include "UnityCG.cginc"
	#include "uSkyPostEffect.cginc"

	UNITY_DECLARE_SHADOWMAP	(u_CascadedShadowMap);
	uniform float3			_OcclusionDarkness;
	uniform float			_SkyRefDistance;

	struct v2f {
		float4 pos		: SV_POSITION;
		float2 uv		: TEXCOORD0;
		float3 ray		: TEXCOORD2;
	};
	
	v2f vert(appdata_img v) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord.xy;
		o.ray = ViewportCorners (v.texcoord.xy);
		return o;
	}

	/**
	 * Gets the cascade weights based on the world position of the fragment and the poisitions of the split spheres for each cascade.
	 * Returns a float4 with only one component set that corresponds to the appropriate cascade.
	 */
	inline fixed4 getCascadeWeights_splitSpheres(float3 wpos) {
		float3 fromCenter0 = wpos.xyz - unity_ShadowSplitSpheres[0].xyz;
		float3 fromCenter1 = wpos.xyz - unity_ShadowSplitSpheres[1].xyz;
		float3 fromCenter2 = wpos.xyz - unity_ShadowSplitSpheres[2].xyz;
		float3 fromCenter3 = wpos.xyz - unity_ShadowSplitSpheres[3].xyz;
		float4 distances2 = float4(dot(fromCenter0,fromCenter0), dot(fromCenter1,fromCenter1), dot(fromCenter2,fromCenter2), dot(fromCenter3,fromCenter3));
		fixed4 weights = float4(distances2 < unity_ShadowSplitSqRadii);
		weights.yzw = saturate(weights.yzw - weights.xyz);
		return weights;
	}

	/**
	* Returns the shadowmap coordinates for the given fragment based on the world position and z-depth.
	* These coordinates belong to the shadowmap atlas that contains the maps for all cascades.
	*/
	inline float4 getShadowCoord(float4 wpos, fixed4 cascadeWeights) {
		float3 sc0 = mul(unity_WorldToShadow[0], wpos).xyz;
		float3 sc1 = mul(unity_WorldToShadow[1], wpos).xyz;
		float3 sc2 = mul(unity_WorldToShadow[2], wpos).xyz;
		float3 sc3 = mul(unity_WorldToShadow[3], wpos).xyz;
		return float4(sc0 * cascadeWeights[0] + sc1 * cascadeWeights[1] + sc2 * cascadeWeights[2] + sc3 * cascadeWeights[3], 1);
	}

	float frag_collect(const v2f i, const int it) {
		const float itF = 1.f / (float)it;
		const float itFM1 = 1.f / (float)(it - 1);
		
		float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);

		float occlusion = 0.f;
		
		float depth = Linear01Depth(rawDepth);
		float3 worldDir = i.ray * depth;
		float4 worldPos = float4(_WorldSpaceCameraPos + worldDir, 1.f);
		float3 deltaStep = -worldDir * itFM1;

		for (int i = 0; i < it; ++i, worldPos.xyz += min(deltaStep, worldPos.z)) 
		{
			float4 cascadeWeights = getCascadeWeights_splitSpheres(worldPos.xyz);
			float3 samplePos = getShadowCoord(worldPos, cascadeWeights);
			occlusion += UNITY_SAMPLE_SHADOW(u_CascadedShadowMap, samplePos) ;
		}
		return occlusion * itF;
	}
	
	fixed4 frag_collect64(v2f i) : SV_Target { return frag_collect(i, 64); }
	fixed4 frag_collect164(v2f i) : SV_Target { return frag_collect(i, 164); }
	fixed4 frag_collect244(v2f i) : SV_Target { return frag_collect(i, 244); }

ENDCG

SubShader {
	ZTest Always Cull Off ZWrite Off
	
	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag_collect64
		ENDCG
	}
	
	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag_collect164
		ENDCG
	}
	
	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag_collect244
		ENDCG
	}
}
Fallback off
}

