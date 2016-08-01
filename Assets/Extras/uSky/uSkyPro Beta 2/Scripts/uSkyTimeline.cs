using UnityEngine;
using System;
using usky.Internal;

namespace usky
{
	/// <summary>
	/// This is the sun and moon direction controller that is responsible for triggerring Time changes.
	/// This script needs to be attached to a GameObject.
	/// It will work as standalone component with uSkySun and uSkyMoon.
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("uSkyPro/uSky Timeline")]
	[DisallowMultipleComponent]
	public class uSkyTimeline : MonoBehaviour {

		public static uSkyTimeline instance { get; private set; }

		[SerializeField] private TimeSettingsMode type = TimeSettingsMode.Default;

		[Space (5)][Range(0.0f, 24.0f)][Tooltip ("Time of the day.")]
		[SerializeField] private float timeline = 17.0f;

		[SerializeField][HeaderLayout]
		private DefaultTimelineSettings sunAndMoon = new DefaultTimelineSettings 
		{
			sunDirection = 0f,
			sunEquatorOffset = 0f,
			moonPositionOffset = 0f
		};

		[SerializeField][HeaderLayout]
		private RealisticTimelineSettings locationAndDate = new RealisticTimelineSettings 
		{
			latitude = 0.0f,
			longitude = 0.0f,
			day = 0,
			month = 4, 
			year = 2016, // <---  need "Year" to expose in the Inspector? then remove the hide flag in uSkySettings.cs and disable the auto sync code on Awake().
			GMTOffset = 0
		};

		[SerializeField][HeaderLayout]
		private DayNightCycleSettings dayNightCycle = new DayNightCycleSettings 
		{
			playAtRuntime = false,
			cycleSpeedCurve = new AnimationCurve (new Keyframe (0.0f, 1.25f), new Keyframe (0.4f, 1.25f), new Keyframe (0.5f, 1.0f), new Keyframe (1.0f, 1.0f)),
			playSpeed = 0.005f,
			steppedInterval = 2f,
		};

		uSkySun m_Sun	{ get{ return uSkySun.instance; }}
		uSkyMoon m_Moon { get{ return uSkyMoon.instance;}}

		Matrix4x4 m_SpaceMatrix;

		[SerializeField][HideInInspector]
		private float m_AccumulatedTime, m_ActualTime;

		void Awake ()
		{
			Year = DateTime.Now.Year; // <--- automatically sync the Year from current system

			uSkyInternal.InitTimelineParameters (this);
		}

		void OnEnable ()
		{
			UpdateSunAndMoon ();
			UpdateTimeline ();

			if(instance && instance != this)
				Debug.LogErrorFormat("Unexpected: uSkyTimeline.instance already set (to: {0}). Still overriding with: {1}.", instance.name, name);

			instance = this;

			uSkyInternal.UpdateTimelineEvent.AddListener (UpdateSunAndMoon);
			uSkyInternal.UpdateCycleEvent.AddListener (UpdateTimeline); 
		}

		void OnDisable() 
		{
			uSkyInternal.UpdateTimelineEvent.RemoveListener (UpdateSunAndMoon);
			uSkyInternal.UpdateCycleEvent.RemoveListener (UpdateTimeline); 

			instance = null;
		}

		void Update () 
		{
			SetTimelineState ();

			if (PlayAtRuntime && Application.isPlaying)
				UpdateTimeCycle ();
		}

		// Set and check if any timeline parameter is dirty,
		// If dirty, trigger event to update sun and moon.
		void SetTimelineState ()
		{
			uSkyInternal.SetTimeMode ((int)Type);
			uSkyInternal.SetTimeline (Timeline);

			switch (Type) 
			{
			case TimeSettingsMode.Realistic:
				uSkyInternal.SetLocationAndDateState (locationAndDate);
				break;
			default:
				uSkyInternal.SetLocationAndDateState (sunAndMoon);
				break;
			}
		}

		// If dirty, Update is called by UpdateTimelineEvent
		void UpdateSunAndMoon ()
		{
			switch (Type) 
			{
			case TimeSettingsMode.Realistic:
				SetSunAndMoonRealisticPosition ();
				break;
			default:
				SetSunAndMoonDirection ();
				break;
			}

			// trigger the events
			uSkyInternal.MarkLightingStateDirty ();
			uSkyInternal.MarkAtmosphereStateDirty ();

//			Debug.Log ("Timeline : Update Sun And Moon.");
		}

		void SetSpaceAndStarsRotation (Quaternion rotation) 
		{
			m_SpaceMatrix = Matrix4x4.identity;
			m_SpaceMatrix.SetTRS ( Vector3.zero, rotation, Vector3.one );
			Shader.SetGlobalMatrix ("_StarRotationMatrix", (uSkyInternal.NightSkyMode == 1) ? m_SpaceMatrix : Matrix4x4.identity);
			Shader.SetGlobalMatrix ("_SpaceRotationMatrix", (uSkyInternal.NightSkyMode == 1) ? m_SpaceMatrix.inverse : Matrix4x4.identity);
		}
	
		void UpdateTimeCycle()
		{
			m_AccumulatedTime += Time.deltaTime;

			m_ActualTime += Time.deltaTime * PlaySpeed * CycleSpeedCurve.Evaluate( uSkyInternal.NormalizedTime (m_Sun, m_Moon) );

			if (m_ActualTime > 24) {
				m_ActualTime = 0;

				DateIncrement ();
			}

			if (m_AccumulatedTime >= SteppedInterval)
			{
				Timeline = m_ActualTime;
				m_AccumulatedTime = 0f;
			}
		}

		/// Allow dynamically update the timeline during playing the cycle
		/// Update is called by UpdateCycleEvent
		void UpdateTimeline()
		{
			m_ActualTime = Timeline;
		}

		void DateIncrement ()
		{
			// Increment Day
			Day += 1;

			// Increment Month and Year 
			if (Day > DaysInMonth ()) {
				Day = 1;
				Month += 1;
			}
			if (Month > 12) {
				Month = 1;
				Year += 1;
			}
		}
	
		int DaysInMonth ()
		{
			return DateTime.DaysInMonth (Year, Month);
		}

		void OnValidate() 
		{
			if (instance == this) 
			{
				Timeline			= Mathf.Clamp (Timeline, 0f, 24f);
				SunDirection		= Mathf.Clamp (SunDirection, -180f, 180f);
				SunEquatorOffset	= Mathf.Clamp (SunEquatorOffset, -60f, 60f);
				MoonPositionOffset	= Mathf.Clamp (MoonPositionOffset, -60f, 60f);

				Latitude			= Mathf.Clamp (Latitude, -90f, 90f);
				Longitude			= Mathf.Clamp (Longitude, -180f, 180f);
				Day					= Mathf.Clamp (Day, 1, DaysInMonth ());
				Month				= Mathf.Clamp (Month, 1, 12);
				Year				= Mathf.Clamp (Year, 1901, 2099);
				GMTOffset			= Mathf.Clamp (GMTOffset, -14, 14);

				PlaySpeed			= Mathf.Max (PlaySpeed, 0f);
				SteppedInterval		= Mathf.Max (SteppedInterval, 0f);
			}
		}

// DEFAULT SETTINGS : SUN AND MOON 
		#region SUN AND MOON
		void SetSunAndMoonDirection() 
		{
			float t = Timeline * 360.0f / 24.0f - 90.0f;
			Quaternion sunEuler = Quaternion.Euler (0f, SunDirection - 90.0f, SunEquatorOffset) * Quaternion.Euler (t, 0f, 0f);

			if (m_Sun) 
			{
				m_Sun.transform.rotation = sunEuler;		// sun
				SetSpaceAndStarsRotation (sunEuler);		// space and stars
			}

			if ((uSkyInternal.NightSkyMode == 1 || uSkyPro.instance == null) && m_Moon)
			{
				Quaternion moonEuler = sunEuler * Quaternion.Euler (new Vector3 (180f, MoonPositionOffset, 180f));
				m_Moon.transform.rotation = moonEuler;		// moon
			}

//			Debug.Log ("Sun and Moon direction updated");
		}
		#endregion


// REALISTIC SETTINGS : LOCATION AND DATE
		#region LOCATION AND DATE

		[HideInInspector]
		private float m_LST, m_Sin_Lat, m_Cos_Lat;

		void SetSunAndMoonRealisticPosition()
		{
			float latitude = Mathf.Deg2Rad * Latitude;
			m_Sin_Lat = Mathf.Sin (latitude);
			m_Cos_Lat = Mathf.Cos (latitude);
			
			float hour = Timeline - GMTOffset;
			
			// http://www.stjarnhimlen.se/comp/ppcomp.html
			
			// Time scale (only works between 1901 to 2099)
			float d = 367 * Year - 7 * (Year + (Month + 9) / 12) / 4 + 275 * Month / 9 + Day - 730530 + hour / 24;
			
			// obliquity of the ecliptic (Tilt of earth's axis of rotation)
			float oblecl = 23.4393f - 3.563E-7f * d;
			float ecl_rad = Mathf.Deg2Rad * oblecl;
			float sin_ecl = Mathf.Sin (ecl_rad);
			float cos_ecl = Mathf.Cos (ecl_rad);
			
			if (m_Sun) 
			{
				m_Sun.transform.forward = computeSunPosition (d, hour, sin_ecl, cos_ecl);	// sun
			
				Quaternion rotation = Quaternion.Euler(90 - Latitude, 0, 0) * Quaternion.Euler(0, Longitude, 0) * Quaternion.Euler(0, m_LST * Mathf.Rad2Deg, 0);
				SetSpaceAndStarsRotation (rotation);										// space and stars
			}
				
			if ((uSkyInternal.NightSkyMode == 1 || uSkyPro.instance == null) && m_Moon)
				m_Moon.transform.forward = computeMoonPosition (d, hour, sin_ecl, cos_ecl);	// moon

//			Debug.Log ("Solar and Lunar position have been updated");
		}

		// Sun
		Vector3 computeSunPosition (float d, float hour, float m_Sin_Ecl, float m_Cos_Ecl)
		{	
			// (all in degrees)
			float w = 282.9404f + 4.70935E-5f	* d;
			float a = 1.0f;
			float e = 0.016709f - 1.151E-9f		* d;
			float M = 356.0470f + 0.9856002585f	* d;
			
			// #5
			float v = 0.0f;
			float r = 0.0f;
			distanceAndTrueAnomaly( Mathf.Deg2Rad * M, e, a, ref v, ref r );
			
			float Ls = v + w;								// Longitude of Sun			(degrees)
			
			localSiderealTime (Ls, hour);
			
			float Ls_rad = Ls * Mathf.Deg2Rad;
			
			float xs = r * Mathf.Cos (Ls_rad);
			float ys = r * Mathf.Sin (Ls_rad);
			
			float xe = xs;
			float ye = ys * m_Cos_Ecl;
			float ze = ys * m_Sin_Ecl;
			
			float RA = Mathf.Atan2 (ye, xe);				// Right Ascension of Sun	(radians)
			float Dec = Mathf.Asin (ze);					// Declination of Sun		(radians)
//			float Dec = Mathf.Atan2 (ze, Mathf.Sqrt (xe*xe+ye*ye));		// Alt formula
			
			Vector2 Sun_PhiTheta = azimuthalCoordinates (RA, Dec);
			
			return cartesianCoordinates (Sun_PhiTheta) *-1;
		}

		// Moon
		Vector3 computeMoonPosition (float d, float hour, float m_Sin_Ecl, float m_Cos_Ecl)
		{			
			float N = 125.1228f - 0.0529538083f	* d;
			float i = 5.1454f;
			float w = 318.0634f + 0.1643573223f * d;
			float a = 60.2666f;
			float e = 0.054900f;
			float M = 115.3654f + 13.0649929509f * d;
			
			// #6
			float v = 0.0f;
			float r = 0.0f;
			distanceAndTrueAnomaly( Mathf.Deg2Rad * M, e, a, ref v, ref r );
			
			float vw_rad = Mathf.Deg2Rad * (v + w);
			float sin_vw = Mathf.Sin(vw_rad);
			float cos_vw = Mathf.Cos(vw_rad);
			
			float N_rad = Mathf.Deg2Rad * N;
			float sin_N = Mathf.Sin(N_rad);
			float cos_N = Mathf.Cos(N_rad);
			
			float i_rad = Mathf.Deg2Rad * i;
			float sin_i = Mathf.Sin(i_rad);
			float cos_i = Mathf.Cos(i_rad);
			
			// #7
			float xh = r * (cos_N * cos_vw - sin_N * sin_vw * cos_i);
			float yh = r * (sin_N * cos_vw + cos_N * sin_vw * cos_i);
			float zh = r * (sin_vw * sin_i);
			
			// #12
			float xe = xh;
			float ye = yh * m_Cos_Ecl - zh * m_Sin_Ecl;
			float ze = yh * m_Sin_Ecl + zh * m_Cos_Ecl;
			
			float RA = Mathf.Atan2(ye, xe);							// Right Ascension of Moon	(radians)
			float Dec = Mathf.Atan2(ze, Mathf.Sqrt(xe*xe + ye*ye));	// Declination of Moon		(radians)
			
			Vector2 Moon_PhiTheta = azimuthalCoordinates (RA, Dec);
			
			return cartesianCoordinates (Moon_PhiTheta)*-1;
		}

		void distanceAndTrueAnomaly (float M, float e, float a, ref float v, ref float r )
		{
			// #5 & #6
			float E = M + e * Mathf.Sin (M) * (1f + e * Mathf.Cos (M));
			
			float xv = a * (Mathf.Cos (E) - e);
			float yv = a * (Mathf.Sqrt (1f - e * e) * Mathf.Sin (E));
			
			v = Mathf.Rad2Deg * Mathf.Atan2 (yv, xv);
			r = Mathf.Sqrt (xv * xv + yv * yv);
		}
		
		void localSiderealTime (float Longitude_Sun, float Hour)
		{
			// #5b
			float UT = 15.0f * Hour;
			float GMST0 = Longitude_Sun + 180.0f;
			float GMST = GMST0 + UT;
			
			m_LST = (GMST + Longitude) * Mathf.Deg2Rad ;
		}
		
		Vector2 azimuthalCoordinates(float RightAscension, float Declination)
		{
			// #12b
			float HA = m_LST - RightAscension;				// Hour Angle
			float cos_Decl = Mathf.Cos (Declination);
			
			float x = Mathf.Cos (HA) * cos_Decl;
			float y = Mathf.Sin (HA) * cos_Decl;
			float z = Mathf.Sin (Declination);
			
			float xhor = x * m_Sin_Lat - z * m_Cos_Lat;
			float yhor = y;
			float zhor = x * m_Cos_Lat + z * m_Sin_Lat;
			
			float azimuth= Mathf.Atan2 (yhor, xhor) + Mathf.PI;
			float altitude = Mathf.Asin (zhor);
			
			float phi = azimuth;
			float theta = (Mathf.PI * 0.5f) - altitude;
			
			return new Vector2 (phi,theta);
		}

		Vector3 cartesianCoordinates(Vector2 Phi_Theta)
		{
			Vector3 v;
			float cosPhi	= Mathf.Cos( Phi_Theta.x );
			float sinPhi	= Mathf.Sin( Phi_Theta.x );
			float cosTheta	= Mathf.Cos( Phi_Theta.y );
			float sinTheta	= Mathf.Sin( Phi_Theta.y );
			
			v.x = sinPhi * sinTheta;
			v.y = cosTheta;
			v.z = cosPhi * sinTheta;
			
			return v;
		}
		#endregion


// Properties accessor --------------------
		#region Properties accessor

		/// <summary>
		/// Switch between Default or Realistic type in Timeline Settings.
		/// </summary>
		public TimeSettingsMode Type
		{
			get { return type; }
			set { type = value; }
		}

		/// <summary>
		/// Time of the day. Range (0 ~ 24)
		/// </summary>
		public float Timeline 
		{
			get { return timeline; }
			set { 
				if (!Application.isPlaying) 
					timeline = value; 
				else
					if (value >= 24f)
						timeline = 0.0f ;
					else 
						if (value < 0.0f)
							timeline = 24.0f;
						else
							timeline = value; 
			}
		}

		/// <summary>
		/// Sun direction align horizionally. Range (-180 ~ 180)
		/// </summary>
		public float SunDirection 
		{
			get { return sunAndMoon.sunDirection; }
			set { sunAndMoon.sunDirection = value;}
		}

		/// <summary>
		/// Sun Path offset. Range (-60 ~ 60)
		/// </summary>
		public float SunEquatorOffset 
		{
			get { return sunAndMoon.sunEquatorOffset; }
			set { sunAndMoon.sunEquatorOffset = value;}
		}

		/// <summary>
		/// The moon position offset in "Rotation" night sky. 
		/// Range (-60 ~ 60)
		/// </summary>
		public float MoonPositionOffset 
		{
			get { return sunAndMoon.moonPositionOffset; }
			set { sunAndMoon.moonPositionOffset = value;}
		}

		/// <summary>
		/// Latitude. Range (-90 ~ 90)
		/// </summary>
		public float Latitude 
		{
			get { return locationAndDate.latitude; }
			set { locationAndDate.latitude = value;}
		}

		/// <summary>
		/// Longitude. Range (-180 ~ 180)
		/// </summary>
		public float Longitude 
		{
			get { return locationAndDate.longitude; }
			set { locationAndDate.longitude = value;}
		}

		/// <summary>
		/// Day. Range (1 ~ 31)
		/// </summary>
		public int Day 
		{
			get { return locationAndDate.day; }
			set 
			{ 
				if (value > 0) 
					locationAndDate.day = value;
			}
		}

		/// <summary>
		/// Month. Range (1 ~ 12)
		/// </summary>
		public int Month 
		{
			get { return locationAndDate.month; }
			set 
			{ 
				if (value > 0)
					locationAndDate.month = value;
			}
		}

		/// <summary>
		/// Year. Range (1901 ~ 2099)
		/// </summary>
		public int Year 
		{
			get { return locationAndDate.year; }
			set 
			{ 
				if ((value > 1900) && (value < 2100))
					locationAndDate.year = value;
			}
		}

		/// <summary>
		/// UTC / Time Zone.
		/// </summary>
		public int GMTOffset 
		{
			get { return locationAndDate.GMTOffset; }
			set 
			{ 
				if ((value > -15) && (value < 15))
					locationAndDate.GMTOffset = value;
			}
		}

// Day Night Cycle -------------------------

		/// <summary>
		/// Enable to Play the Day Night Cycle at runtime
		/// </summary>
		public bool PlayAtRuntime
		{
			get { return dayNightCycle.playAtRuntime; }
			set { dayNightCycle.playAtRuntime = value;}
		}

		public float PlaySpeed
		{
			get { return dayNightCycle.playSpeed;}
			set { dayNightCycle.playSpeed = value;}
		}

		public AnimationCurve CycleSpeedCurve
		{
			get { return dayNightCycle.cycleSpeedCurve;}
			set { dayNightCycle.cycleSpeedCurve = value;}
		}

		public float SteppedInterval
		{
			get { return dayNightCycle.steppedInterval;}
			set { dayNightCycle.steppedInterval = value; }
		}

		#endregion

		/*
		// Formatted: Time to TimeSpan , Date to DateTime
		
		public TimeSpan uSkyTimeSpan { 
			get { return TimeSpan.FromHours ((double)Timeline); }
		}

		public DateTime uSkyDateTime {
			get {
				DateTime date = new DateTime ( Year, Month, Day );
				return date.Add (uSkyTime);
			}
		}
		*/

	} // end class
}