Shader "Hidden/uSkyPro/uStars" {

	CGINCLUDE
							
	#if defined(UNITY_COLORSPACE_GAMMA)
		#define OUTPUT(color) color
	#else
		#define OUTPUT(color) (color*color)*2
	#endif
		
	uniform float		_StarIntensity;
	uniform float4x4	_StarRotationMatrix;
	uniform float2 		_tab[8];
	
	struct appdata_t {
		float4 vertex		: POSITION;
		float4 ColorAndMag	: COLOR;
		float2 texcoord		: TEXCOORD;
	};
	
	struct v2f 
	{
		float4 pos	: SV_POSITION;
		half4 Color	: COLOR;
		half2 uv	: TEXCOORD0;
	};	
	
	float GetFlickerAmount(in float2 pos)
	{
	#ifndef ENABLE_STARS_FIX
		const float2 tab[8] = 
		{
			float2(0.897907815,-0.347608525), float2(0.550299290, 0.273586675), float2(0.823885965, 0.098853070), float2(0.922739035,-0.122108860),
			float2(0.800630175,-0.088956800), float2(0.711673375, 0.158864420), float2(0.870537795, 0.085484560), float2(0.956022355,-0.058114540)
		};
	#endif
		float2 hash = frac(pos.xy * 256);
		float index = frac(hash.x + (hash.y + 1) * (_Time.x * 2 + unity_DeltaTime.z)); // flickering speed
		index *= 8;

		float f = frac(index)* 2.5;
		int i = (int)index;
		
	#ifdef ENABLE_STARS_FIX
		// using uniform _tab array
		// array will be assigned by script
		return _tab[i].x + f * _tab[i].y;
	#else
		// using default const tab array. 
		// occasionally this is not working for WebGL and some android build
		return tab[i].x + f * tab[i].y;
	#endif
	}	
	
	v2f vert(appdata_t v)
	{
		v2f OUT = (v2f)0;

		float3 t = mul((float3x3)_StarRotationMatrix, v.vertex.xyz) + _WorldSpaceCameraPos.xyz; 
		OUT.pos = mul(UNITY_MATRIX_MVP, float4 (t, 1))  ;

		float appMag = 6.5 + v.ColorAndMag.w * (-1.44 -1.5);
		float brightness = GetFlickerAmount(v.vertex.xy) * pow(5.0, (-appMag -1.44)/ 2.5);
		
		OUT.Color = _StarIntensity * float4( brightness * v.ColorAndMag.xyz, brightness );
		OUT.uv = 6.5 * v.texcoord.xy - 6.5 * float2(0.5, 0.5);
		
		return OUT;
	}

	half4 frag(v2f IN) : SV_Target
	{
		half2 distCenter = IN.uv.xy;
		half scale = exp(-dot(distCenter, distCenter));
		half3 col = IN.Color.xyz * scale + 5 * IN.Color.w * pow(scale, 10);
		col = OUTPUT(col);
		return half4(col, 0);
	}
	ENDCG
//----------------------------------------------
SubShader {
	Tags { "Queue"="Geometry+502" "IgnoreProjector"="True" "RenderType"="Background" }
	
	Blend OneMinusDstAlpha  OneMinusSrcAlpha	// alpha 0
//	Blend OneMinusDstAlpha  SrcAlpha			// alpha 1

	ZWrite Off

Pass{	
	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#pragma multi_compile __ ENABLE_STARS_FIX	
	#pragma multi_compile __ UNITY_COLORSPACE_GAMMA

	ENDCG
  }
 }
}
