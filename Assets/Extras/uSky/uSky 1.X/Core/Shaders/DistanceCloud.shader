Shader "uSky/DistanceCloud" {
Properties {

    Attenuation ("Attenuation", Range(0,5)) = 0.6
    StepSize ("Step size", Range(0.001,0.02)) = 0.004
    AlphaSaturation("Alpha saturation", Range(1,10)) = 2.0
    SunColorMultiplier ("Sun Color multiplier", Range(0,8)) = 4
    SkyColorMultiplier("Sky Color multiplier", Range(0,8)) = 1.5
	[Enum(Rectangular,0,Polar,1)] _Mapping ("Mapping mode", Float) = 0	
	CloudSampler ("Texture (R)", 2D) = "white" {}
	
	// "Opacity mask" is for blocking the color behind the cloud, useful at night time.
	// if using "Opacity mask", then requires cloud texture's channel "g" for masking.
	Mask ("Opacity mask (G)", Range (0,1)) = 0.0
	// Range 0 ~ 360 for non-animated Rotation
    RotateSpeed("Rotate speed", Range (-1,1)) = 0.0 
}
SubShader {
		Tags { "Queue"="Geometry+501" "RenderType"="Background" }

		Blend  SrcAlpha OneMinusSrcAlpha

		Zwrite Off  

Pass {
		Name "BASE"
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		UNITY_DECLARE_TEX2D (CloudSampler);
		uniform float3 _SunDir, _MoonDir, _SkyMultiplier;
		uniform float RotateSpeed, _Mapping;
		uniform half Attenuation, StepSize, AlphaSaturation, SunColorMultiplier, SkyColorMultiplier, Mask;
		uniform half2 _colorCorrection;
		uniform half3 ShadeColorFromSun, ShadeColorFromSky;
		float4 CloudSampler_ST;

		struct appdata_t {
			float4	vertex		: POSITION;
			float4	tangent		: TANGENT;
			float3	normal		: NORMAL;
    		float2  rectangular	: TEXCOORD0; 
    		float2  polar		: TEXCOORD1; 
		};
		
		struct v2f {
		    float4	pos 	: SV_POSITION;
		    half2	baseTC	: TEXCOORD0;
		    half2	toSun	: TEXCOORD1;
		};

		float3 RotateAroundYInDegrees (float3 vertex, float degrees)
		{
			float alpha = degrees * 3.1416 / 180.0;
			float sina, cosa;
			sincos(alpha, sina, cosa);
			float2x2 m = float2x2(cosa, -sina, sina, cosa);
			return float3(mul(m, vertex.xz), vertex.y).xzy;
		}

		v2f vert (appdata_t v)
		{
		    v2f o;
			float offsetValue = RotateSpeed *_Time.y+ unity_DeltaTime.z;
		    float3 t = RotateAroundYInDegrees(v.vertex.xyz, offsetValue).xyz; //  animate rotation 
//			t.y = pow(v.vertex.y, 0.85 );
			t = t * _ProjectionParams.z + _WorldSpaceCameraPos.xyz ; //  camera’s far plane
			
			o.pos = mul (UNITY_MATRIX_MVP, float4(t,1));
			o.pos.z = o.pos.w; // render behind all other objects

			// switching between the sun and moon direction, avoids the poping issue between lights
			float3 lightDir = lerp (_SunDir, _MoonDir, saturate( _SkyMultiplier.z - 0.1));
			// Inverse rotation to correct the light direction from vertex animation
			float3 dir = RotateAroundYInDegrees(lightDir, -offsetValue);
	
			TANGENT_SPACE_ROTATION;
			o.toSun = mul(rotation, dir).xy * StepSize;

			if ( _Mapping == 0 )
				o.baseTC = TRANSFORM_TEX (v.rectangular, CloudSampler);
			else 
				o.baseTC = v.polar ;
			
		    return o;
		}

		half4 frag (v2f i) : SV_Target
		{
			const int c_numSamples = 8; //  keep in SM 2.0
			
			half2 sampleDir = i.toSun.xy ;
			half2 uv = i.baseTC.xy;
			
			// r = cloud density , g = Opacity mask
			half2 opacity = UNITY_SAMPLE_TEX2D( CloudSampler, uv ).rg; 
			
			if (Mask > 0.02)
				opacity.r = lerp(opacity.r, opacity.g, Mask); 

			half density = 0;
			
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