using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;

namespace uSky
{
	[AddComponentMenu("uSky/uSkyGUI Helper")]
	public class uSkyGUI_Helper : MonoBehaviour {

		  
		public Text TimeDisplay = null;
		public Slider[] slider; //  total #13

		uSkyManager uSM;

		// for uGUI slider
		public void SetTimeline(float value)
		{
			if( uSM )
				uSM.Timeline = value;
		}

		public void SetSunDirection (float value)
		{
			if( uSM )
				uSM.SunDirection = value;
		}

		public void SetExposure (float value)
		{
			if( uSM )
				uSM.Exposure = value;
		}

		public void SetRayleigh (float value)
		{
			if( uSM )
				uSM.RayleighScattering = value;
		}

		public void SetMie (float value)
		{
			if( uSM )
				uSM.MieScattering = value;
		}

		public void SetSunAnisotropyFactor (float value)
		{
			if( uSM )
				uSM.SunAnisotropyFactor = value;
		}

		public void SetSunSize (float value)
		{
			if( uSM )
				uSM.SunSize = value;
		}

		public void SetWavelength_X (float value)
		{
			if( uSM )
				uSM.Wavelengths.x = value;
		}

		public void SetWavelength_Y (float value)
		{
			if( uSM )
				uSM.Wavelengths.y = value;
		}

		public void SetWavelength_Z (float value)
		{
			if( uSM )
				uSM.Wavelengths.z = value;
		}

		public void SetStarIntensity (float value)
		{
			if( uSM )
				uSM.StarIntensity = value;
		}

		public void SetOuterSpaceIntensity (float value)
		{
			if( uSM )
				uSM.OuterSpaceIntensity = value;
		}

		public void SetMoonSize (float value)
		{
			if( uSM )
				uSM.MoonSize = value;
		}
		public void SetMoonInnerCoronaScale (float value)
		{
			if( uSM )
				uSM.MoonInnerCorona.a = value;
		}
		public void SetMoonOuterCoronaScale (float value)
		{
			if( uSM )
				uSM.MoonOuterCorona.a = value;
		}

		void Start () {
			uSM = uSkyManager.instance;
			if (uSM ) 
				uSM.SkyUpdate = true;
		}
		
		void Update () {


			if ( TimeDisplay && uSM ) {
				TimeSpan t = TimeSpan.FromHours ((double) uSM.Timeline); 
//				TimeDisplay.text = string.Format ("{0:D2}:{1:D2}", t.Hours, t.Minutes);  // GC Alloc: 11 / 408 B

				StringBuilder strTime = new StringBuilder ();
				strTime.Append (t.Hours.ToString("D2"));
				strTime.Append (":");
				strTime.Append (t.Minutes.ToString("D2"));
				TimeDisplay.text = strTime.ToString (); // GC Alloc: 5 / 234 B
			}
		}

		public void Reset_uSky () {

			slider[0].value = 1.0f;
			slider[1].value = 1.0f;
			slider[2].value = 1.0f;
			slider[3].value = 0.76f;
			slider[4].value = 1.0f;

			slider[5].value = 680f;
			slider[6].value = 550f;
			slider[7].value = 440f;

			slider[8].value = 1.0f;
			slider[9].value = 0.25f;
			slider[10].value = 0.15f;

			slider[11].value = 0.5f;
			slider[12].value = 0.5f;
		}
	}
}