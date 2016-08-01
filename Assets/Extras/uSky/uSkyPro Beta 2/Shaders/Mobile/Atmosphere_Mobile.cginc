/*=============================================================================
	This code contains embedded portions of free sample source code from 
	http://www-evasion.imag.fr/Membres/Eric.Bruneton/PrecomputedAtmosphericScattering2.zip, Author: Eric Bruneton, 
	08/16/2011, Copyright (c) 2008 INRIA, All Rights Reserved, which have been altered from their original version.

	Permission is granted to anyone to use this software for any purpose, including commercial applications, and to alter it and redistribute it freely, subject to the following restrictions:

    1. Redistributions of source code must retain the above copyright notice, 
	   this list of conditions and the following disclaimer.
    2. Redistributions in binary form must reproduce the above copyright notice, 
	   this list of conditions and the following disclaimer in the
       documentation and/or other materials provided with the distribution.
    3. Neither the name of the copyright holders nor the names of its
       contributors may be used to endorse or promote products derived from
       this software without specific prior written permission.
       
	Author: Eric Bruneton
	Modified by Michael Lam 2015
=============================================================================*/
#ifndef USKY_ATMOSPHERE_MOBILE
#define USKY_ATMOSPHERE_MOBILE

const static float Rg = 6360000.0;
const static float Rt = 6420000.0;
const static float RL = 6421000.0;

const static int RES_R		= 1; 	// 3D texture depth
const static int RES_MU		= 128; 	// height of the texture
const static int RES_MU_S	= 32; 	// width per table
const static int RES_NU		= 8;	// table per texture depth

#define SUN_BRIGHTNESS (40.0 * _uSkyExposure)

UNITY_DECLARE_TEX2D		(_Transmittance);
UNITY_DECLARE_TEX2D		(_Inscatter);
UNITY_DECLARE_TEX2D		(_MoonSampler);
UNITY_DECLARE_TEXCUBE	(_OuterSpaceCube);

// x = NightFade, y = MoonFade, z = OuterSpaceIntensity 
uniform half3 		_uSkyNightParams;

uniform half 		_uSkyExposure;
uniform half4		betaR;

uniform	float		_uSkyMieG;
uniform	float		_uSkyMieScale;
uniform	float		_uSkyGroundOffset;
uniform	float		_uSkyAltitudeScale;
uniform float4		_SunDirSize;
uniform float4		_MoonDirSize;

uniform half4		_NightZenithColor;
uniform half4		_NightHorizonColor;
uniform half4		_MoonInnerCorona;
uniform half4		_MoonOuterCorona;

uniform float4x4	_SpaceRotationMatrix;
			
// ---------------------------------------------------------------------------- 
// VERTEX FUNCTIONS 
// ----------------------------------------------------------------------------
	
float Get_uMuS (float muS)
{
   	return float(0.5 / float(RES_MU_S) + (atan(max(muS, -0.1975) * tan(1.26 * 1.1)) / 1.1 + (1.0 - 0.26)) * 0.5 * (1.0 - 1.0 / float(RES_MU_S)));
	
}			
									
float3 PhaseFunctionG(float g, float MieScale) 
{
	float g2 = g * g;
	return float3(MieScale * 1.5 * 1.0 / (4.0 * UNITY_PI) * ((1.0 - g2) / (2.0 + g2)), 1.0 + g2, 2.0 * g);
}

// ---------------------------------------------------------------------------- 
// FRAGMENT FUNCTIONS 
// ----------------------------------------------------------------------------
								
// ported from unity tonemapping: Photographic formular
half3 HDRtoLDR (half3 c)
{
	return 1.0 - exp2(-(_uSkyExposure.x * 1.5) * c);
}
				
half3 GetMie(half4 rayMie) 
{	
   	return rayMie.rgb * rayMie.w / max(rayMie.r, 1e-4) * (betaR.r / betaR.xyz);
}

half PhaseFunctionR()
{
	return 3.0 / (16.0 * UNITY_PI);
}

half PhaseFunctionM(half mu, half3 miePhase_g) 
{
	return miePhase_g.x / pow( miePhase_g.y - miePhase_g.z * mu, 1.5 );
}

half3 Texture2D_Mobile_2 (float skyDir, float2 Mu_uMuS, half nu, half3 miePhase_g, out half3 extinction)
{
	half Mu	  = Mu_uMuS.x;
	half uMuS = Mu_uMuS.y;

	// read ground level extinction data only
	extinction = UNITY_SAMPLE_TEX2D(_Transmittance, float2(skyDir + 0.71, 0.0)).rgb;
	
	// getting sky gradient artifacts if we calculate uMu in vertex 
	// so we calculate the uMu in fragment get better smooth sky gradient result.
	half uMu = 0.5 + (-Mu + sqrt(Mu * Mu + 0.766))/ 1.85 ;
	
	// no earth shadow
    half4 inscatter = UNITY_SAMPLE_TEX2D(_Inscatter, half2( uMuS, uMu));			    
	
	half3 inscatterM = GetMie(inscatter);
	half phaseR = PhaseFunctionR();
	half phaseM = PhaseFunctionM(nu, miePhase_g);
																																								
	return half3(inscatter.rgb * phaseR + inscatterM * phaseM) * ((1.0 + nu * nu)* SUN_BRIGHTNESS);

}

#endif
