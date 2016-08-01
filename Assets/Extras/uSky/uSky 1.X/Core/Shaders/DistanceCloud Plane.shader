// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// This shader will work with Sprite Renderer too.

Shader "uSky/DistanceCloud Plane" {
Properties {
//	[HideInInspector][PerRendererData] _MainTex ("Dummy",2D) = "white" {} // for Sprite Renderer
    Attenuation ("Attenuation", Range(0,5)) = 0.6
    StepSize ("Step size", Range(0.001,0.02)) = 0.004
    AlphaSaturation("Alpha saturation", Range(1,10)) = 2.0
    SunColorMultiplier ("Sun Color multiplier", Range(0,8)) = 4
    SkyColorMultiplier("Sky Color multiplier", Range(0,8)) = 1.5
	CloudSampler ("Texture (R)", 2D) = "white" {}
	Mask ("Opacity mask (G)", Range (0,1)) = 0.0

}
SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }

		Blend  SrcAlpha OneMinusSrcAlpha

		Zwrite Off  Cull Off

Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"
		
		UNITY_DECLARE_TEX2D (CloudSampler);
		uniform float3 _SunDir, _MoonDir, _SkyMultiplier; 
		uniform half Attenuation, StepSize, AlphaSaturation, SunColorMultiplier, SkyColorMultiplier, Mask;
		uniform half3 ShadeColorFromSun, ShadeColorFromSky;
		float4 CloudSampler_ST;

		struct v2f {
		    float4	pos 	: SV_POSITION;
		    half2	baseTC	: TEXCOORD0;
		    half3	toSun	: TEXCOORD1;
		};

		v2f vert (appdata_tan v)
		{
		    v2f OUT;

			OUT.pos = mul (UNITY_MATRIX_MVP, float4(v.vertex));
			OUT.baseTC = v.texcoord;
			
			// switching between the sun and moon direction, avoids the poping issue between lights
			float3 lightDir = lerp (_SunDir, _MoonDir, saturate( _SkyMultiplier.z - 0.1));
			float3 objectSpaceLightPos = mul((float3x3)unity_WorldToObject, lightDir).xyz;
			
			TANGENT_SPACE_ROTATION;
			OUT.toSun = mul(rotation, objectSpaceLightPos);
		    return OUT;
		}

		half4 frag (v2f IN) : SV_Target
		{
			const int c_numSamples = 8;
			
			half3 toSun = normalize( IN.toSun.xyz );
			half2 sampleDir = toSun.xy * StepSize; 
			half2 uv = IN.baseTC.xy;
			
			// r = cloud density , g = Opacity mask
			half2 opacity = UNITY_SAMPLE_TEX2D( CloudSampler, uv ).rg; 
			
			if (Mask > 0.02)
				opacity.r = lerp(opacity.r, opacity.g, Mask); 

			half density = 0.0;
			
			for( int i = 0; i < c_numSamples; i++ )
			{
				half2 sampleUV = uv + i * sampleDir;
				half t = UNITY_SAMPLE_TEX2D( CloudSampler, sampleUV ).r ;
				density += t ;
			}

			half c = exp2( -Attenuation * density);
			half a = pow( opacity.r, AlphaSaturation );
			half3 col = lerp( SkyColorMultiplier * ShadeColorFromSky.xyz, SunColorMultiplier * ShadeColorFromSun.xyz, c );
			
			return half4( col, a ) ;
			
		}
		ENDCG

    }
}
Fallback Off
} 