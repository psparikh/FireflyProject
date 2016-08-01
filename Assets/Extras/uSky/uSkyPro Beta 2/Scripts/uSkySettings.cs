using UnityEngine;
using System;

namespace usky
{
	/// <summary>
	/// This is the settings used through out uSky
	/// </summary>

	/// <summary>
	/// The Night Sky type settings for uSkyPro
	/// </summary>
	public enum NightModes { Static = 0, Rotation = 1 }

	/// <summary>
	/// The number of altitudes sample .
	/// </summary>
	public enum DepthSample { X1 = 1, /* X2 = 2 , */ X4 = 4 }

	/// <summary>
	/// The Time settings type between Default and Realistic for uSkyTimeline
	/// </summary>
	public enum TimeSettingsMode { Default = 0, Realistic = 1 }


	public enum OcclusionDownscaleMode { x1 = 1, x2 = 2, x4 = 4 }
	public enum OcclusionSamplesMode { x64 = 0, x164 = 1, x244 = 2 }

//	public enum TextureType { Transmittance = 0, Inscatter = 1 }


	/// <summary>
	/// Night sky settings for uSkyPro.
	/// </summary>
	[Serializable]
	public struct NightSkySettings 
	{
		public NightModes nightMode;

		[Tooltip ("The zenith color of the night sky. (Top of the night sky)")]
		public Color nightZenithColor;

		[Tooltip ("The horizon color of the night sky gradient.\nThis Alpha value controls the night fog height in skybox")]
		public Color nightHorizonColor;

		[Range(0.0f, 5.0f)][Tooltip ("Control the intensity of the Star field in night sky.\nIf this value is at zero, stars rendering will be disabled")]
		public float starIntensity;

		[Range(0.0f, 2.0f)][Tooltip ("This controls the intensity of the Outer Space Cubemap in night sky.")]
		public float outerSpaceIntensity;

		[Tooltip ("The color of the moon's inner corona.\n This Alpha value controls the size and blurriness corona.")]
		public Color moonInnerCorona;

		[Tooltip ("The color of the moon's outer corona.\nThis Alpha value controls the size and blurriness corona.")]
		public Color moonOuterCorona;

		[Range(0.0f, 5.0f)][Tooltip ("This controls the moon texture size in the night sky.")]
		public float moonSize;
	}

	/// <summary>
	/// Other settings for uSkyPro
	/// </summary>
	[Serializable]
	public struct OtherSettings
	{
		[Range(0.0f, 10000.0f)][Tooltip ("This is an independent altitude offset for camera view position in Skybox.\nHigher the value means the height of the camera view position will offset to atmosphere level.")]
		public float groundOffset;

		[Range(0.0f,20.0f)][Tooltip ("This is a current camera height position multiplier in Skybox.\nHigher the value means the camera height position will move faster to atmosphere level." +
			"\n\nIf the value is 0, the camera is always viewing at the sea level, and skybox horizion will not produce any curve effect." +
			"\nIf both ground offset and altitude scale are always set to 0 value, in this case that recommended just to use Altitude Sample X1 for better rendering performance.")]
		public float altitudeScale;

		[Tooltip ("Disable the default ocean rendering effect on the Skybox.\nThere are no performance gain by disabled it.")]
		public bool disableSkyboxOcean;

		[Tooltip ("Toggle it if the Main Camera is using HDR mode and Tonemapping image effect.")]
		public bool HDRMode;
	}

	/// <summary>
	/// Precompute settings for uSkyPro
	/// </summary>
	[Serializable]
	public struct PrecomputeSettings
	{
		// TODO;
		// The AtmosphereThickness value higher than 1.8 products ugly artifacts on Windows platform.
		// So temporary we clamped the maximum to 1.8 for now until fixed the artifacts issue.
		[Range (0f, 1.8f)][Tooltip ("Rayleigh scattering density scale. Increase this value produces typical earth-like sky colors (reddish/yellowish colors at sun set, and the like).")]
		public float atmosphereThickness;

		[Tooltip ("It is visible spectrum light waves (range 380 to 780).\n\nTweaking these values will shift the colors of the resulting gradients and produce different kinds of atmospheres.")]
		public Vector3 wavelengths; // sea level mie

		[Tooltip ("Tweaking this color value is simular to change the Wavelengths value to shift the colors of the sky.\nThis is just for more artist friendly control of wavelengths.")]
		public Color skyTint;

		[Tooltip ("Number of different altitudes at which to sample inscatter color.\n\nX1 is a single inscatter sample for ground level only.\nIt uses also less calculation in the shader for better rendering performance. (Texture Memory: 256KB)" +
			"\n\nX4 includes atmosphere level inscatter samples that divided into four samples, which allows camera to travel nearby atmosphere on the earth with correct inscatter color. (Texture Memory: 1.0MB)" +
			"\n\nNote : Not includes outer space sample. (Rendering will be out of range if the camera goes outside the atmosphere.)")]
		public DepthSample inscatterAltitudeSample;
	}

	/// <summary>
	/// Default settings for uSkyTimeline
	/// </summary>
	[Serializable]
	public struct DefaultTimelineSettings
	{
		[Range(-180.0f, 180.0f)]
		[Tooltip ("Controls the sun light direction align horizionally.")]
		public float sunDirection;

		[Range(-60.0f, 60.0f)]
		[Tooltip ("Controls sun path offset")]
		public float sunEquatorOffset;

		[Range(-60.0f, 60.0f)]
		[Tooltip ("Controls the moon position in \"Rotation\" night sky.\n\nIf night sky is \"Static\", the moon can be rotate freely with rotation tool in Editor.")]
		public float moonPositionOffset;

	}

	/// <summary>
	/// Realistic settings for uSkyTimeline
	/// </summary>
	[Serializable]
	public struct RealisticTimelineSettings
	{
		[Range (-90.0f, 90.0f)]
		public float latitude;

		[Range (-180.0f, 180.0f)]
		public float longitude;

		[Range(1,31)][Tooltip("Note: The value of maximum day in month will dynamically clamped according to which month of the year.")]
		public int day;

		[Range(1,12)]
		public int month;

		[HideInInspector]// <---  need "Year" to expose in the Inspector? then remove the hide flag
		public int year; 

		[Range(-14, 14)][Tooltip ("UTC / Time Zone")]
		public int GMTOffset;
	}

	/// <summary>
	/// Play cycle setting for uSkyTimeline
	/// </summary>
	[Serializable]
	public struct DayNightCycleSettings
	{
		public bool playAtRuntime;

		[Tooltip ("Controls how fast the play speed during day and night cycles.\n\nDefault time curve keys have been set to 25% faster at night." +
		"\n\nCurve key value of 1 means no speed change, and if key value is higher means the play speed is faster, vice versa.")]
		public AnimationCurve cycleSpeedCurve;

		[Tooltip("Controls how much the distance when sun moves between Interval update.")]
		public float playSpeed;

		[Tooltip ("Update Timeline per second." +
		"\n\nBy default the Reflection Probe refresh mode is set to \"All Face At Once\" which spreads update over 9 frames. So the minimum value will be arround 0.3." +
		"\n\nIf this value is 0 means it will update everyframe, in that case the Reflection Probe refresh mode should set to \"No Time Slicing\"")]
		public float steppedInterval;

	}

	/// <summary>
	/// Ambient gradients for uSkyLighting
	/// </summary>
	[Serializable] 
	public struct AmbientGradientSettings
	{
		[Tooltip ("Enabled this toggle to switch RenderSettings ambient source to gradient mode." +
			"\n\nDisable it if using the skybox ambient lighting from Lighting window setting.\n(Currently skybox ambient lighting doesn't work correctly in this Beta version)")]
		public bool useGradientMode;

		[Tooltip("Ambient lighting coming from above.")]
		public Gradient SkyColor;

		[Tooltip("Ambient lighting coming from side.")]

		public Gradient EquatorColor;
		[Tooltip("Ambient lighting coming from below.")]
		public Gradient GroundColor;

	}
	/// <summary>
	/// World inscatter settings for uSkyAtmosphericScattering.
	/// </summary>
	[Serializable]
	public struct WorldInscatterSettings
	{
		[Tooltip ("Enable World Atmospheric Scattering Image Effect")]
		public bool enablePostEffect;

		[Tooltip ("Control the inscattering intensity on the earth")]
		[Range (0.0f, 5.0f)]
		public float scatteringIntensity;

		[Tooltip ("How much light is out-scattered or absorbed on the earth. Basically how much to darken the shaded pixel.")]
		[Range (0.0f, 1.0f)]
		public float scatterExtinction;

		[Tooltip ("Control the world scale factor for the scattering on the earth")]
		public float worldScatterScale;

		[Tooltip("Allows the scattering to be pushed out to have no scattering directly in front of the camera, or pulled in to have more scattering close to the camera.")]
		public float nearScatterPush;

	}

	/// <summary>
	/// Scatter occlusion settings for uSkyAtmosphericScattering.
	/// </summary>
	[Serializable]
	public struct ScatterOcclusionSettings
	{
		[Tooltip ("This flag enables scatter occlusion")]
		public bool	useOcclusion;

		[Tooltip ("Controls how dark the occlusion on earth.\nA value of 0 results full occlusion darkening.\nA value of 1 results no darkening term.")]
		[Range (0.0f,1.0f)]
		public float					occlusionDarkness;

		[Tooltip ("Controls the down sample scale of the screen resolution for occlusion gathering.")]
		public OcclusionDownscaleMode	occlusionDownscale;

		[Tooltip ("The number of samples to use in occlusion gathering.")]
		public OcclusionSamplesMode		occlusionSamples;
	}
}