/*=============================================================================
	Atmosphere.cginc : Functions and variables only used in Atmospheric Scattering

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
	Modified and ported to Unity by Justin Hawkins 2014
	Modified by Michael Lam 2015
=============================================================================*/
#ifndef USKY_ATMOSPHERE_INCLUDED
#define USKY_ATMOSPHERE_INCLUDED

const static float HR = 8.0;
const static float HM = 1.2;
const static float3 betaMSca = float3(4e-3, 4e-3, 4e-3);
const static float3 betaMEx = betaMSca / 0.9;

UNITY_DECLARE_TEX2D (_Transmittance);
UNITY_DECLARE_TEX2D (_Inscatter);

uniform float	_uSkyExposure;
uniform float	_uSkyMieG;
uniform float	_uSkyMieScale;
// x = NightFade, y = MoonFade, z = OuterSpaceIntensity 
uniform float3	_uSkyNightParams;
uniform float4	betaR;
uniform float4	_SunDirSize;
uniform float	_uSkyGroundOffset;
uniform float	_uSkyAltitudeScale;
uniform float	_uSkyAtmosphereThickness;
uniform int		_uSkySkyboxOcean;

// Image effects parameters
uniform float _AtmosphereFogMultiplier, _AtmosphereWorldScale, _NearScatterPush;

//#define M_PI 3.141592 // use UNITY_PI
const static float Rg = 6360000.0;
const static float Rt = 6420000.0;
const static float RL = 6421000.0;

uniform int RES_R; 				// 3D texture depth
const static int RES_MU = 128; 	// height of the texture
const static int RES_MU_S = 32; // width per table
const static int RES_NU = 8;	// table per texture depth

const static float3 EARTH_POS = float3(0.0, 6360010.0, 0.0);
const static half SUN_BRIGHTNESS = 40.0 * _uSkyExposure;

#define TRANSMITTANCE_NON_LINEAR
#define INSCATTER_NON_LINEAR

/* Whether to attempt to fix small pixels artifacts appears at the horizion rendering.
 * Enable this option will cost more calculation in shader. 
 */
#define HORIZON_FIX

//--------------------------------------------------------------------------------------------------

half3 hdr(half3 L) 
{
    L.r = L.r < 1.413 ? pow(L.r * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.r);
    L.g = L.g < 1.413 ? pow(L.g * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.g);
    L.b = L.b < 1.413 ? pow(L.b * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.b);
    return L;
}
// switch different tonemapping methods between day and night
half3 hdr2(half3 L) 
{
    L = lerp(hdr(L),1.0 - exp(-L), _uSkyNightParams.x);
    return L;
}
half3 HDRtoRGB (half3 c)
{
	return 1 - exp2(-(_uSkyExposure * 1.5) * c);
}

//float4 Texture4D(sampler2D table, float r, float mu, float muS, float nu)
float4 Texture4D(float r, float mu, float muS, float nu)
{
   	float H = sqrt(Rt * Rt - Rg * Rg);
   	float rho = sqrt(r * r - Rg * Rg);
#ifdef INSCATTER_NON_LINEAR
    float rmu = r * mu;
    float delta = rmu * rmu - r * r + Rg * Rg;
    float4 cst = rmu < 0.0 && delta > 0.0 ? float4(1.0, 0.0, 0.0, 0.5 - 0.5 / RES_MU) : float4(-1.0, H * H, H, 0.5 + 0.5 / RES_MU);     
    float uR = 0.5 / RES_R + rho / H * (1.0 - 1.0 / RES_R);
    float uMu = cst.w + (rmu * cst.x + sqrt(delta + cst.y)) / (rho + cst.z) * (0.5 - 1.0 / float(RES_MU));

if (_uSkySkyboxOcean == 0)
	uMu = rmu < 0.0 && delta > 0.0 ? 0.975 : uMu * 0.975;	// 0.975 to fix the horizion seam issue.
//	uMu = rmu < 0.0 && delta > 0.0 ? 0.975 : uMu - 0.015;	// Alt

    // paper formula
    //float uMuS = 0.5 / RES_MU_S + max((1.0 - exp(-3.0 * muS - 0.6)) / (1.0 - exp(-3.6)), 0.0) * (1.0 - 1.0 / RES_MU_S);
    // better formula
    float uMuS = 0.5 / RES_MU_S + (atan(max(muS, -0.1975) * tan(1.26 * 1.1)) / 1.1 + (1.0 - 0.26)) * 0.5 * (1.0 - 1.0 / RES_MU_S);
#else
    float uR = 0.5 / RES_R + rho / H * (1.0 - 1.0 / RES_R);
    float uMu = 0.5 / RES_MU + (mu + 1.0) / 2.0 * (1.0 - 1.0 / RES_MU);
    float uMuS = 0.5 / RES_MU_S + max(muS + 0.2, 0.0) / 1.2 * (1.0 - 1.0 / RES_MU_S);
#endif
    float lep = (nu + 1.0) / 2.0 * (RES_NU - 1.0);
    float uNu = floor(lep);
    lep = lep - uNu;

    //Original 3D lookup
    //return tex3D(table, float3((uNu + uMuS) / RES_NU, uMu, uR)) * (1.0 - lep) + tex3D(table, float3((uNu + uMuS + 1.0) / RES_NU, uMu, uR)) * lep;

#ifdef USKY_MULTISAMPLE  
    //new 2D lookup
	float u_0 = floor(uR*RES_R)/RES_R;
	float u_1 = floor(uR*RES_R+1.0)/RES_R;
	float u_frac = frac(uR*RES_R);

	float4 A = UNITY_SAMPLE_TEX2D(_Inscatter, float2((uNu + uMuS) / RES_NU, uMu / RES_R + u_0)) * (1.0 - lep) + UNITY_SAMPLE_TEX2D(_Inscatter, float2((uNu + uMuS + 1.0) / RES_NU, uMu / RES_R + u_0)) * lep;	
	float4 B = UNITY_SAMPLE_TEX2D(_Inscatter, float2((uNu + uMuS) / RES_NU, uMu / RES_R + u_1)) * (1.0 - lep) + UNITY_SAMPLE_TEX2D(_Inscatter, float2((uNu + uMuS + 1.0) / RES_NU, uMu / RES_R + u_1)) * lep;	

	return A * (1.0-u_frac) + B * u_frac;

#else	
	return UNITY_SAMPLE_TEX2D(_Inscatter, float2((uNu + uMuS) / RES_NU, uMu)) * (1.0 - lep) + UNITY_SAMPLE_TEX2D(_Inscatter, float2((uNu + uMuS + 1.0) / RES_NU, uMu)) * lep;	
#endif
}

//--------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------
float3 GetMie(float4 rayMie) 
{	
	// approximated single Mie scattering (cf. approximate Cm in paragraph "Angular precision")
	// rayMie.rgb=C*, rayMie.w=Cm,r
   	return rayMie.rgb * rayMie.w / max(rayMie.r, 1e-4) * (betaR.r / betaR.xyz);
}
float PhaseFunctionR(float mu) 
{
	// Rayleigh phase function
    return (3.0 / (16.0 * UNITY_PI)) * (1.0 + mu * mu);
}
float PhaseFunctionR() 
{
	// Rayleigh phase function without multiply (1.0 + mu * mu)
    return 3.0 / (16.0 * UNITY_PI);
}
/*
float PhaseFunctionM(float mu) // original code
{
	// Mie phase function
  return 1.5 * 1.0 / (4.0 * UNITY_PI) * (1.0 - mieG*mieG) * pow(1.0 + (mieG*mieG) - 2.0*mieG*mu, -3.0/2.0) * (1.0 + mu * mu) / (2.0 + mieG*mieG);
}
*/
float PhaseFunctionM(float mu, float3 miePhase_g)  // optimized
{
	// Mie phase function (optimized)
	return miePhase_g.x / pow( miePhase_g.y - miePhase_g.z * mu, 1.5 );
}
// 
float3 PhaseFunctionG(float g, float mieScale) 
{
	// Mie phase G function and Mie scattering scale, (compute this function in Vertex program)
	float g2 = g * g;
	return float3(mieScale * 1.5 * 1.0 / (4.0 * UNITY_PI) * ((1.0 - g2) / (2.0 + g2)), 1.0 + g2, 2.0 * g);
}
// ---------------------------------------------------------------------------- 
// TRANSMITTANCE FUNCTIONS 
// ---------------------------------------------------------------------------- 
	// transmittance(=transparency) of atmosphere for infinite ray (r,mu)
	// (mu=cos(view zenith angle)), intersections with ground ignored
float3 Transmittance(float r, float mu) 
{
   	float uR, uMu;
#ifdef TRANSMITTANCE_NON_LINEAR
    uR = sqrt((r - Rg) / (Rt - Rg));
    uMu = atan((mu + 0.15) / (1.0 + 0.15) * tan(1.5)) / 1.5;
#else
    uR = (r - Rg) / (Rt - Rg);
    uMu = (mu + 0.15) / (1.0 + 0.15);
#endif    
    return UNITY_SAMPLE_TEX2D(_Transmittance, float2(uMu, uR)).rgb;
}


// ---------------------------------------------------------------------------- 
// INSCATTER FUNCTIONS (SKYBOX)
// ---------------------------------------------------------------------------- 
	// scattered sunlight between two points
	// camera=observer
	// viewdir=unit vector towards observed point
	// sundir=unit vector towards the sun
	// return scattered light
	
	// optimized scattering phase formula

float3 SkyRadiance(float3 camera, float3 viewdir, float3 MiePhase_g, out float3 extinction)
{
	camera += EARTH_POS;

   	float3 result = float3(0,0,0);
    float r = length(camera);
    float rMu = dot(camera, viewdir);
    float mu = rMu / r ;

    float deltaSq = sqrt(rMu * rMu - r * r + Rt*Rt);
    float din = max(-rMu - deltaSq, 0.0);
    if (din > 0.0) 
    {
       	camera += din * viewdir;
       	rMu += din;
       	mu = rMu / Rt;
       	r = Rt;
    }
    
    float nu = dot(viewdir, _SunDirSize.xyz);
    float muS = dot(camera, _SunDirSize.xyz) / r;

    float4 inScatter = Texture4D(r, rMu / r, muS, nu);
    extinction = Transmittance(r, mu);

    if(r <= Rt ) 
    {
        float3 inScatterM = GetMie(inScatter);
        float phase = PhaseFunctionR();
        float phaseM = PhaseFunctionM(nu, MiePhase_g);
        result = (inScatter.rgb * phase + inScatterM * phaseM)*(1.0 + nu * nu);
    }
    else
    {
    	result = float3(0,0,0);
    	extinction = float3(1,1,1);
    }

    return result * SUN_BRIGHTNESS;
}

// ---------------------------------------------------------------------------- 
// INSCATTER FUNCTIONS (IMAGE EFFECTS)
// ---------------------------------------------------------------------------- 
	// single scattered sunlight between two points
	// camera=observer
	// point=point on the ground
	// sundir=unit vector towards the sun
	// return scattered light and extinction coefficient
	
	// optimized scattering phase formula
	 
float3 InScattering(float3 camera, float3 _point, float3 MiePhase_g, inout float3 extinction ) 
{
    float3 result = float3(0,0,0);

    float worldScale = max( _AtmosphereWorldScale, 1.0);

	camera.y	*= worldScale;
	_point		*= worldScale;
    camera.y	+= Rg;
    _point.y	+= Rg ;
                                                                                         
    float3 viewdir = _point - camera; 
    float d = length(viewdir);
    viewdir = viewdir / d;
    float r = length(camera);

    float rMu = dot(camera, viewdir); 
    float mu = rMu / r;   
    _point -= viewdir * clamp(_NearScatterPush, 0.0, d);

    float deltaSq = sqrt(rMu * rMu - r * r + Rt*Rt);
    float din = max(-rMu - deltaSq, 0.0);
    
    if (din > 0.0) // if camera in space and ray intersects atmosphere
    {
        camera += din * viewdir;
        rMu += din;
        mu = rMu / Rt;
        r = Rt;
        d -= din;
    }

    if (r <= Rt) // if ray intersects atmosphere
    {
        float nu = dot(viewdir, _SunDirSize.xyz);
        float muS = dot(camera, _SunDirSize.xyz) / r; 

        float4 inScatter;
        
		// avoids artifact issue when atmosphere thickness value is too high
		float HeightOffset = Rg + 1500 * _uSkyAtmosphereThickness;
		 
        if (r < HeightOffset) 
        {
            // avoids imprecision problems in aerial perspective near ground
            float f = HeightOffset / r;
            r = r * f;
            rMu = rMu * f;
            _point = _point * f;
        }

        float r1 = length(_point);
        float rMu1 = dot(_point, viewdir);
        float mu1 = rMu1 / r1;
        float muS1 = dot(_point, _SunDirSize.xyz) / r1;
           
        if (mu > 0.0) 
            extinction = min(Transmittance(r, mu) / Transmittance(r1, mu1), 1.0);
        else 
            extinction = min(Transmittance(r1, -mu1) / Transmittance(r, -mu), 1.0);

#ifdef HORIZON_FIX
		// avoids imprecision problems near horizon by interpolating between two points above and below horizon
        const float EPS = 0.004;
        float lim = -sqrt(1.0 - (Rg / r) * (Rg / r));
        
        if (abs(mu - lim) < EPS) 
        {
            float a = ((mu - lim) + EPS) / (2.0 * EPS);

            mu = lim - EPS;
            r1 = sqrt(r * r + d * d + 2.0 * r * d * mu);
            mu1 = (r * mu + d) / r1;
            
            float4 inScatter0 = Texture4D(r, mu, muS, nu);
            float4 inScatter1 = Texture4D(r1, mu1, muS1, nu);
            float4 inScatterA = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);
/*
            mu = lim + EPS;
            r1 = sqrt(r * r + d * d + 2.0 * r * d * mu);
            mu1 = (r * mu + d) / r1;
            
            inScatter0 = Texture4D(r, mu, muS, nu);
            inScatter1 = Texture4D(r1, mu1, muS1, nu);
            float4 inScatterB = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);

            inScatter = lerp(inScatterA, inScatterB, a);
*/

			inScatter = inScatterA; // temp: fix for OpenGLCore compiler error in Unity 5.3.2p3 / 5.4 or newer

        } 
        else
#endif
        {
            float4 inScatter0 = Texture4D(r, mu, muS, nu);
            float4 inScatter1 = Texture4D(r1, mu1, muS1, nu);
            inScatter = max(inScatter0 - inScatter1 * extinction.rgbr, 0.0);
        }
        
        // avoids imprecision problems in Mie scattering when sun is below horizon
        inScatter.w *= smoothstep(0.00, 0.02, muS);

        float3 inScatterM = GetMie(inScatter);
        float phaseR = PhaseFunctionR();
        float phaseM = PhaseFunctionM(nu, MiePhase_g);

        result = (inScatter.rgb * phaseR + inScatterM * phaseM)*(1.0 + nu * nu);        
    } else { // camera in space and ray looking in space
        result = float3(0,0,0);
        extinction = float3(1,1,1);
    }

    return result * SUN_BRIGHTNESS ;
}

#endif // USKY_ATMOSPHERE_INCLUDED