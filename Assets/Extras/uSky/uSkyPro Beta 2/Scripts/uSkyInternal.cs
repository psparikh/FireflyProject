using UnityEngine;
using UnityEngine.Events;

namespace usky.Internal
{
	/// <summary>
	/// This script cached all the parameters internally for uSkyPro package.
	/// In general you should not access to this script, unless you know what you are doing.
	/// In most case you should just call uSkyPro.instance instead.
	/// </summary>
	public static class uSkyInternal 
	{
		// Unity Events
		public static UnityEvent UpdateTimelineEvent	= new UnityEvent(); // uSkyTimeline					: UpdateSunAndMoon ()
		public static UnityEvent UpdateCycleEvent		= new UnityEvent(); // uSkyTimeline					: SetTimelineState ()
		public static UnityEvent UpdateLightingEvent	= new UnityEvent(); // uSkyLighting					: UpdateDirectLighting ()
		public static UnityEvent UpdateAtmosphereEvent	= new UnityEvent();	// uSkyPro						: UpdateMaterialUniform ()
		public static UnityEvent UpdatePrecomputedEvent	= new UnityEvent(); // uSkyPro						: UpdatePrecomputeData ()
		public static UnityEvent UpdateProbeEvent		= new UnityEvent(); // uSkyReflectionProbeUpdater	: RenderReflectionProbe ()

		/// <summary>
		/// Trigger the event for Sun and Moon position update ()
		/// </summary>
		public static void MarkTimelineStateDirty ()
		{
			if(UpdateTimelineEvent != null)
				UpdateTimelineEvent.Invoke ();
		}

		/// <summary>
		/// Trigger the event for ActualTime to Timeline in uSkyTimeline
		/// </summary>
		public static void MarkCycleStateDirty ()
		{
			if (UpdateCycleEvent != null)
				UpdateCycleEvent.Invoke ();
		}

		/// <summary>
		/// Trigger the event for Sun and Moon light color and intensity update.
		/// </summary>
		public static void MarkLightingStateDirty ()
		{
			if (UpdateLightingEvent != null )
				UpdateLightingEvent.Invoke ();
		}

		/// <summary>
		/// Trigger the event for Skybox and Atmospheric Scattering update.
		/// </summary>
		public static void MarkAtmosphereStateDirty ()
		{
			if (UpdateAtmosphereEvent != null)
				UpdateAtmosphereEvent.Invoke ();
		}

		/// <summary>
		/// Trigger the event for Precomputed Data update.
		/// </summary>
		public static void MarkPrecomputedStateDirty ()
		{
			if (UpdatePrecomputedEvent != null)
				UpdatePrecomputedEvent.Invoke ();
		}

		/// <summary>
		/// Trigger the event for Reflection probe cubemap
		/// </summary>
		public static void MarkProbeStateDirty ()
		{
			if (UpdateProbeEvent != null)
				UpdateProbeEvent.Invoke ();
		}
			
		public static void RemoveAllEventListeners ()
		{
			UpdateTimelineEvent.RemoveAllListeners ();
			UpdateLightingEvent.RemoveAllListeners ();
			UpdateAtmosphereEvent.RemoveAllListeners ();
			UpdatePrecomputedEvent.RemoveAllListeners ();
		}

		/// <summary>
		/// Normalized value of current altitude position of sun
		/// (Range 0.0 to 1.0)
		/// 1	= the sun is at zenith.
		/// 0.5	= the sun is at the horizon.
		/// 0	= the sun is at the bottom.
		/// </summary>
		public static float NormalizedTime (uSkySun m_Sun, uSkyMoon m_Moon)
		{ 
			float value = 1f;
			if (m_Sun)
				value = (-m_Sun.transform.forward.y + 1f) * 0.5f;
			else if (m_Moon)
				value = (m_Moon.transform.forward.y + 1f) * 0.5f;

			return value;
		}

		// Sky Parameters
		internal static float	Exposure;
		internal static float	MieScattering;
		internal static float	SunAnisotropyFactor;
		internal static float	SunSize;
		internal static float	StarIntensity;
		internal static float	OuterSpaceIntensity;
		internal static float	MoonSize;
		internal static float	GroundOffset;
		internal static float	AltitudeScale;

		internal static Color	GroundColor;
		internal static Color	NightZenithColor;
		internal static Color	NightHorizonColor;
		internal static Color	MoonInnerCorona;
		internal static Color	MoonOuterCorona;

		internal static int 	NightSkyMode;

		internal static bool	DisableSkyboxOcean;
		internal static bool	HDRMode;

		// Precomputed Parameters
		internal static float	AtmosphereThickness;
		internal static Vector3 Wavelengths;
		internal static Color	SkyTint;
		internal static int		InscatterAltitudeSample;

		// Timeline Parameters
		internal static int		TimeMode;
		internal static float	Timeline;

		internal static float	SunDirection;
		internal static float	SunEquatorOffset;
		internal static float	moonPositionOffset;
			
		internal static	float	Latitude;
		internal static	float	Longitude;

		internal static int 	Day;
		internal static int 	Month;
		internal static int 	Year;
		internal static int 	GMTOffset;

		#region uSkyPro Parameters
		public static void InitAtmosphereParameters (uSkyPro uSP)
		{
			Exposure				= uSP.Exposure;
			MieScattering			= uSP.MieScattering;
			SunAnisotropyFactor 	= uSP.SunAnisotropyFactor;
			SunSize					= uSP.SunSize;

			NightSkyMode			= (int)uSP.NightMode;
			NightZenithColor 		= uSP.NightZenithColor;
			NightHorizonColor		= uSP.NightHorizonColor;
			StarIntensity			= uSP.StarIntensity;
			OuterSpaceIntensity 	= uSP.OuterSpaceIntensity;
			MoonInnerCorona			= uSP.MoonInnerCorona;
			MoonOuterCorona			= uSP.MoonOuterCorona;
			MoonSize				= uSP.MoonSize;

//			GroundColor				= uSP.GroundColor;
			GroundOffset			= uSP.GroundOffset;
			AltitudeScale			= uSP.AltitudeScale;
			DisableSkyboxOcean		= uSP.DisableSkyboxOcean;
			HDRMode					= uSP.HDRMode;

			// Precomputed params
			AtmosphereThickness		= uSP.AtmosphereThickness;
			Wavelengths				= uSP.Wavelengths;
			SkyTint					= uSP.SkyTint;
			InscatterAltitudeSample	= (int)uSP.InscatterAltitudeSample;
		}
			
		public static void SetNightSkyMode (int NewNightSkyMode)
		{
			if (NightSkyMode != NewNightSkyMode) {
				NightSkyMode = NewNightSkyMode;
				// trigger Timeline to update the Space and Stars rotation
				MarkTimelineStateDirty ();
				// Update moon element in skybox
				MarkAtmosphereStateDirty ();
			}
		}

		public static void SetSkyboxOcean (bool NewSkyboxOcean)
		{
			if (DisableSkyboxOcean != NewSkyboxOcean) {
				DisableSkyboxOcean = NewSkyboxOcean;
				MarkAtmosphereStateDirty ();
//				MarkProbeStateDirty ();
			}
		}

		public static void SetHDRMode (bool NewHDRMode)
		{
			if (HDRMode != NewHDRMode) {
				HDRMode = NewHDRMode;
				MarkProbeStateDirty ();
			}
		}
		public static void SetAtmosphereParameterState (uSkyPro uSP) 
		{
			// uSkyPro Parameters
			SetExposure				(uSP.Exposure);
			SetMieScattering		(uSP.MieScattering);
			SetSunAnisotropyFactor	(uSP.SunAnisotropyFactor);
			SetSunSize				(uSP.SunSize);

//			SetNightSkyMode			((int)uSP.NightMode);
			SetNightZenithColor		(uSP.NightZenithColor);
			SetNightHorizonColor	(uSP.NightHorizonColor);
			SetStarIntensity		(uSP.StarIntensity);
			SetOuterSpaceIntensity	(uSP.OuterSpaceIntensity);
			SetMoonInnerCorona		(uSP.MoonInnerCorona);
			SetMoonOuterCorona		(uSP.MoonOuterCorona);
			SetMoonSize				(uSP.MoonSize);
			
//			SetGroundColor			(uSP.GroundColor);
			SetGroundOffset			(uSP.GroundOffset);
			SetAltitudeScale		(uSP.AltitudeScale);
			SetSkyboxOcean 			(uSP.DisableSkyboxOcean);

			// Precomputed params
			SetAtmosphereThickness	(uSP.AtmosphereThickness);
			SetWavelengths			(uSP.Wavelengths);
			SetSkyTint				(uSP.SkyTint);
			SetInscatterAltitudeSample ((int)uSP.InscatterAltitudeSample);

		}

		static void SetExposure (float NewExposure)
		{
			if (Exposure != NewExposure) {
				Exposure = NewExposure;
				MarkAtmosphereStateDirty ();
				MarkLightingStateDirty ();
			}
		}

		static void SetMieScattering (float NewMieScattering)
		{
			if (MieScattering != NewMieScattering){
				MieScattering = NewMieScattering;
				MarkAtmosphereStateDirty ();			
			}
		}
	
		static void SetSunAnisotropyFactor (float NewSunAnisotropyFactor)
		{
			if (SunAnisotropyFactor != NewSunAnisotropyFactor){
				SunAnisotropyFactor = NewSunAnisotropyFactor;
				MarkAtmosphereStateDirty ();
			}
		}
		
		static void SetSunSize (float NewSunSize)
		{
			if (SunSize != NewSunSize) {
				SunSize = NewSunSize;
				MarkAtmosphereStateDirty ();
			}
		}

		static void SetNightZenithColor (Color NewNightZenithColor)
		{
			if (NightZenithColor != NewNightZenithColor){
				NightZenithColor = NewNightZenithColor;
				MarkAtmosphereStateDirty ();
			}
		}
		
		static void SetNightHorizonColor (Color NewNightHorizonColor)
		{
			if (NightHorizonColor != NewNightHorizonColor){
				NightHorizonColor = NewNightHorizonColor;
				MarkAtmosphereStateDirty ();
			}
		}
		
		static void SetStarIntensity (float NewStarIntensity)
		{
			if (StarIntensity != NewStarIntensity){
				StarIntensity = NewStarIntensity;
				Shader.SetGlobalFloat ("_StarIntensity", StarIntensity * 5f);
			}
		}
		
		static void SetOuterSpaceIntensity (float NewOuterSpaceIntensity)
		{
			if (OuterSpaceIntensity != NewOuterSpaceIntensity) {
				OuterSpaceIntensity = NewOuterSpaceIntensity;
				MarkAtmosphereStateDirty ();
			}
		}
		
		static void SetMoonInnerCorona (Color NewMoonInnerCorona)
		{
			if (MoonInnerCorona != NewMoonInnerCorona){
				MoonInnerCorona = NewMoonInnerCorona;
				MarkAtmosphereStateDirty ();
			}
		}
		
		static void SetMoonOuterCorona (Color NewMoonOuterCorona)
		{
			if (MoonOuterCorona != NewMoonOuterCorona){
				MoonOuterCorona = NewMoonOuterCorona;
				MarkAtmosphereStateDirty ();
			}
		}
		
		static void SetMoonSize (float NewMoonSize)
		{
			if (MoonSize != NewMoonSize) {
				MoonSize = NewMoonSize;
				MarkAtmosphereStateDirty ();
			}
		}
		
		static void SetGroundColor (Color NewGroundColor)
		{
			if (GroundColor != NewGroundColor) {
				GroundColor = NewGroundColor;
				MarkAtmosphereStateDirty ();
			}
		}
		static void SetGroundOffset (float NewGroundOffset)
		{
			if (GroundOffset != NewGroundOffset) {
				GroundOffset = NewGroundOffset;
				MarkAtmosphereStateDirty ();
//				Shader.SetGlobalFloat ("_uSkyGroundOffset", GroundOffset * (float)InscatterAltitudeSample);
			}
		}

		static void SetAltitudeScale (float NewAltitudeScale)
		{
			if (AltitudeScale != NewAltitudeScale){
				AltitudeScale = NewAltitudeScale;
				MarkAtmosphereStateDirty ();
//				Shader.SetGlobalFloat ("_uSkyAltitudeScale", AltitudeScale);
			}
		}


// Precomputed parameters ----------------------

		static void SetAtmosphereThickness(float NewAtmosphereThickness)
		{
			if (AtmosphereThickness != NewAtmosphereThickness) {
				AtmosphereThickness = NewAtmosphereThickness;
				MarkPrecomputedStateDirty();
				Shader.SetGlobalFloat ("_uSkyAtmosphereThickness", AtmosphereThickness); // fix the artifact for deferred
			}
		}
		
		static void SetWavelengths (Vector3 NewWavelengths)
		{
			if (Wavelengths != NewWavelengths) {
				Wavelengths = NewWavelengths;
				MarkPrecomputedStateDirty();			
			}
		}
		
		static void SetSkyTint (Color NewSkyTint)
		{
			if (SkyTint != NewSkyTint) {
				SkyTint = NewSkyTint;
				MarkPrecomputedStateDirty();			
			}
		}

		static void SetInscatterAltitudeSample(int NewInscatterAltitudeSample)
		{
			if (InscatterAltitudeSample != NewInscatterAltitudeSample) {
				InscatterAltitudeSample = NewInscatterAltitudeSample;
				MarkPrecomputedStateDirty();				
				Shader.SetGlobalFloat ("_uSkyGroundOffset", GroundOffset * (float)InscatterAltitudeSample);
			}
		}

		#endregion

// TimeLine Settings ------------------------

		#region TimeLine Settings

		public static void InitTimelineParameters (uSkyTimeline uST){
			TimeMode			= (int)uST.Type;
			Timeline			= uST.Timeline;

			SunDirection		= uST.SunDirection;
			SunEquatorOffset	= uST.SunEquatorOffset;
			moonPositionOffset	= uST.MoonPositionOffset;

			Latitude			= uST.Latitude;
			Longitude			= uST.Longitude;

			Day					= uST.Day;
			Month				= uST.Month;
			Year				= uST.Year;
			GMTOffset			= uST.GMTOffset;
		}

		public  static void SetTimeMode (int NewTimeMode)
		{
			if (TimeMode != NewTimeMode) {
				TimeMode = NewTimeMode;
				MarkTimelineStateDirty();
			}
		}

		public static void SetTimeline (float NewTimeline)
		{
			if (Timeline != NewTimeline) {
				Timeline = NewTimeline;
				MarkCycleStateDirty ();
				MarkTimelineStateDirty();
			}
		}

// Default timeline settings --------------------------

		public static void SetLocationAndDateState (DefaultTimelineSettings Setting)
		{
			SetSunDirection (Setting.sunDirection);
			SetSunEquatorOffset (Setting.sunEquatorOffset);
			SetMoonPositionOffset (Setting.moonPositionOffset);

		}

		public static void SetSunDirection (float NewSunDirection)
		{
			if (SunDirection != NewSunDirection) {
				SunDirection = NewSunDirection;
				MarkTimelineStateDirty();
			}
		}
		public static void SetSunEquatorOffset (float NewEquatorOffset)
		{
			if (SunEquatorOffset != NewEquatorOffset) {
				SunEquatorOffset = NewEquatorOffset;
				MarkTimelineStateDirty();
			}
		}
		public static void SetMoonPositionOffset (float NewMoonPositionOffset)
		{
			if (moonPositionOffset != NewMoonPositionOffset) {
				moonPositionOffset = NewMoonPositionOffset;
				MarkTimelineStateDirty();
			}
		}
// Realistic timeline settings --------------------------

		public static void SetLocationAndDateState (RealisticTimelineSettings Setting)
		{
			SetLatitude (Setting.latitude);
			SetLongitude (Setting.longitude);
			SetDay (Setting.day);
			SetMonth (Setting.month);
			SetYear (Setting.year); 
			SetGMTOffset (Setting.GMTOffset);

		}

		static void SetLatitude (float NewLatitude)
		{
			if (Latitude != NewLatitude) {
				Latitude = NewLatitude;
				MarkTimelineStateDirty();
			}
		}
		static void SetLongitude (float NewLongitude)
		{
			if (Longitude != NewLongitude) {
				Longitude = NewLongitude;
				MarkTimelineStateDirty();
			}
		}
		static void SetDay (int NewDay)
		{
			if (Day != NewDay) {
				Day = NewDay;
				MarkTimelineStateDirty();
			}
		}
		static void SetMonth (int NewMonth)
		{
			if (Month != NewMonth) {
				Month = NewMonth;
				MarkTimelineStateDirty();
			}
		}
		static void SetYear (int NewYear)
		{
			if (Year != NewYear) {
				Year = NewYear;
				MarkTimelineStateDirty();
			}
		}
		static void SetGMTOffset (int NewGMTOffset)
		{
			if (GMTOffset != NewGMTOffset) {
				GMTOffset = NewGMTOffset;
				MarkTimelineStateDirty();
			}
		}
		#endregion
	}
}