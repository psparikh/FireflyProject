// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable
// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'

#ifndef USKY_POSTEFFECT
#define USKY_POSTEFFECT

sampler2D_float	_CameraDepthTexture;

#define Deg2Rad	UNITY_PI / 180.0
#define Rad2Deg	180.0 / UNITY_PI

CBUFFER_START(UnityPerCamera2)
// float4x4 _CameraToWorld;
CBUFFER_END

// Based on Unity Image Effects GlobalFog.cs script
float3 FrustumCorners (int index)
{
	float3 camtr	= _WorldSpaceCameraPos.xyz;
	float camNear	= _ProjectionParams.y;
	float camFar	= _ProjectionParams.z;

	float3 camRgt	= mul((float3x3)unity_CameraToWorld, float3(1,0,0));
	float3 camUp	= mul((float3x3)unity_CameraToWorld, float3(0,1,0));
	float3 camFwd	= mul((float3x3)unity_CameraToWorld, float3(0,0,1));

	float camAspect	= unity_CameraProjection[1].y / unity_CameraProjection[0].x;
	
	float t = unity_CameraProjection[1].y;
	float camFov = atan(1.0 / t ) * 2.0 * Rad2Deg;
					
	float fovWHalf	= camFov * 0.5;
	float tanFov	= tan (fovWHalf * Deg2Rad);
	
	float3 toRight	= camRgt * camNear * tanFov * camAspect;
	float3 toTop	= camUp * camNear * tanFov;
	
	float3 topLeft	= (camFwd * camNear - toRight + toTop);
	float camScale	= length (topLeft) * camFar/camNear;
	
	topLeft = normalize(topLeft);
	topLeft *= camScale;
	
	float3 topRight = (camFwd * camNear + toRight + toTop);
	topRight = normalize(topRight);
	topRight *= camScale;
	
	float3 bottomRight = (camFwd * camNear + toRight - toTop);
	bottomRight = normalize(bottomRight);
	bottomRight *= camScale;
	
	float3 bottomLeft = (camFwd * camNear - toRight - toTop);
	bottomLeft = normalize(bottomLeft);
	bottomLeft *= camScale;	
	
	// set row
//	float4x4 frustumCorners;		
//	frustumCorners._m00_m01_m02 = topLeft ; 
//	frustumCorners._m10_m11_m12 = topRight;		
//	frustumCorners._m20_m21_m22 = bottomRight;
//	frustumCorners._m30_m31_m32 = bottomLeft;

	float4x4 frustumCorners = {
		float4(	topLeft,	1),
		float4(	topRight,	1),
		float4(	bottomRight,1),
		float4(	bottomLeft,	1)
	};

	return frustumCorners[index].xyz;
}

// Based on Unity The Black Smith AtmosphericScattering.cs script
float3 ViewportCorners (float2 uv)
{
	// http://forum.unity3d.com/threads/get-main-camera-up-direction.189947/#post-1295774
	float3 camRgt	= mul((float3x3)unity_CameraToWorld, float3(1,0,0));
	float3 camUp	= mul((float3x3)unity_CameraToWorld, float3(0,1,0));
	float3 camFwd	= mul((float3x3)unity_CameraToWorld, float3(0,0,1));
			
	float camTop	= unity_CameraProjection[1].y;
	float camAspect	= camTop / unity_CameraProjection[0].x; 
	
	// https://developer.vuforia.com/forum/unity-3-extension-technical-discussion/vertical-fov-unity
	float fov = atan(1.0 / camTop ) * 2.0 * Rad2Deg;

	float dy = tan (fov * 0.5 * Deg2Rad); 
	float dx = dy * camAspect; 
	
	float camFar	= _ProjectionParams.z;
	float3 vpCenter	= camFwd * camFar;
	float3 vpRight	= camRgt * dx * camFar;
	float3 vpUp		= camUp * dy * camFar;

	float3 u_ViewportCorner	= vpCenter - vpRight - vpUp;
	float3 u_ViewportRight	= vpRight * 2;
	float3 u_ViewportUp		= vpUp * 2;
	
	return u_ViewportCorner + uv.x * u_ViewportRight + uv.y * u_ViewportUp;
}

#endif