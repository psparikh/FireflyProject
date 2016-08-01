using UnityEngine;
using usky.Internal;

namespace usky
{
	/// <summary>
	/// This script is responsible for the direct and ambient lighting of the scene
	/// This script needs to be attached to a GameObject.
	/// It will work as standalone component with uSkySun and uSkyMoon.
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("uSkyPro/uSky Lighting")]
	[DisallowMultipleComponent]
	public class uSkyLighting : MonoBehaviour 
	{	

		[Space()][Tooltip ("The color of the both Sun and Moon light emitted")]
		public Gradient LightColor = new Gradient()
		{
			colorKeys = new GradientColorKey[] {
				new GradientColorKey(new Color32(085, 099, 112, 255), 0.49f),
				new GradientColorKey(new Color32(245, 173, 084, 255), 0.51f),
				new GradientColorKey(new Color32(249, 208, 144, 255), 0.57f),
				new GradientColorKey(new Color32(252, 222, 186, 255), 1.00f),
			},
			alphaKeys = new GradientAlphaKey[] {
				new GradientAlphaKey(1.0f, 0.0f),
				new GradientAlphaKey(1.0f, 1.0f)
			}
		};

		[Range(0f, 8f)][Tooltip ("Brightness of the Sun (directional light)")]
		public float SunIntensity = 1.0f;

		[Range(0f, 1f)][Tooltip ("Brightness of the Moon (directional light). If the Moon Intensity is at 0 (less then 0.01), the Moon light will auto disabled and always disabled at Day time")]
		public float MoonIntensity = 0.2f;

		[HeaderLayout][Tooltip ("Ambient light that shines into the scene.")]
		public AmbientGradientSettings Ambient = new AmbientGradientSettings
		{
			useGradientMode = true,
			
			SkyColor = new Gradient ()
			{
				colorKeys = new GradientColorKey[] {
					new GradientColorKey(new Color32(028, 032, 040, 255), 0.475f),
					new GradientColorKey(new Color32(055, 065, 063, 255), 0.50f),
					new GradientColorKey(new Color32(138, 168, 168, 255), 0.55f),
					new GradientColorKey(new Color32(145, 174, 210, 255), 0.65f),
				},
				alphaKeys = new GradientAlphaKey[] {
					new GradientAlphaKey(1.0f, 0.0f),
					new GradientAlphaKey(1.0f, 1.0f)
				}
			},
			 EquatorColor = new Gradient ()
			{
				colorKeys = new GradientColorKey[] {
					new GradientColorKey(new Color32(017, 021, 030, 255), 0.475f),
					new GradientColorKey(new Color32(100, 100, 078, 255), 0.52f),
					new GradientColorKey(new Color32(128, 150, 168, 255), 0.58f),
				},
				alphaKeys = new GradientAlphaKey[] {
					new GradientAlphaKey(1.0f, 0.0f),
					new GradientAlphaKey(1.0f, 1.0f)
				}
			},
			GroundColor = new Gradient ()
			{
				colorKeys = new GradientColorKey[] {
					new GradientColorKey(new Color32(021, 020, 019, 255), 0.48f),
					new GradientColorKey(new Color32(094, 089, 087, 255), 0.55f),
				},
				alphaKeys = new GradientAlphaKey[] {
					new GradientAlphaKey(1.0f, 0.0f),
					new GradientAlphaKey(1.0f, 1.0f)
				}
			}
		};

		uSkySun m_Sun	{ get{ return uSkySun.instance; }}
		uSkyMoon m_Moon	{ get{ return uSkyMoon.instance;}}
		uSkyPro uSP		{ get{ return uSkyPro.instance; }}

		Light sunLight, moonLight;

		void Awake ()
		{
			InitLightingParameters ();
			UpdateDirectLighting ();
		}

		void OnEnable ()
		{
			SetAmbientMode ();
			uSkyInternal.UpdateLightingEvent.AddListener(UpdateDirectLighting);
		}

		void OnDisable ()
		{
			uSkyInternal.UpdateLightingEvent.RemoveListener(UpdateDirectLighting);
		}

		void Update ()
		{
			// set and check only the light intensity slider value
			SetLightingState ();
		}

		// Update is called by UpdateLightingEvent
		public void UpdateDirectLighting ()
		{
			if (m_Sun && sunLight == null)
				sunLight = m_Sun.GetComponent<Light>();
			
			if (m_Moon && moonLight == null)
				moonLight = m_Moon.GetComponent<Light>();

			if (sunLight)
			{
				float dayLighting = (uSP) ? Mathf.Clamp01 (uSP.DayTimeBrightness() * 4) : 1f;
				sunLight.intensity = SunIntensity * dayLighting;
				sunLight.color = CurrentLightColor ();
				// enable on Day, disable at Night. Always enable if the moon light is off (MoonIntensity = 0)
				sunLight.enabled = (normalizedTime() > 0.48f || MoonIntensity < 0.01) ? true : false;
			}

			if (moonLight) 
			{
				float nightLighting = (uSP) ? uSP.NightTimeBrightness() * uSP.MoonFade() : 1f; 
				moonLight.intensity = MoonIntensity * nightLighting;
				moonLight.color = CurrentLightColor ();
				// Moon Intensity > 0.01 it will auto enable at Night, always disabled at Day time
				moonLight.enabled = (normalizedTime() < 0.50f && MoonIntensity > 0.01) ? true : false;
			}

			// Ambient
			if (RenderSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Trilight && Ambient.useGradientMode)
				AmbientGradientUpdate ();
			else
				RenderSettings.ambientLight = CurrentSkyColor(); // update it for cloud color

//			Debug.Log ("Lighthing updated!");
//			Debug.Log ("Lighthing - NormalizedTime :    " + NormalizedTime);
		}

		float normalizedTime ()
		{
			return uSkyInternal.NormalizedTime (m_Sun, m_Moon); 
		}

		float exposure ()
		{ 
			return (uSP)? Mathf.Pow( uSP.Exposure, 0.4f) : 1f; 
		}

		public Color CurrentLightColor ()
		{
			return LightColor.Evaluate (normalizedTime ()); 
		}

		void AmbientGradientUpdate ()
		{
			RenderSettings.ambientSkyColor		= CurrentSkyColor ();
			RenderSettings.ambientEquatorColor	= CurrentEquatorColor ();
			RenderSettings.ambientGroundColor	= CurrentGroundColor ();
		}

		public Color CurrentSkyColor ()
		{
			return Ambient.SkyColor.Evaluate (normalizedTime ())* exposure (); 
		}

		public Color CurrentEquatorColor ()
		{
			return Ambient.EquatorColor.Evaluate (normalizedTime ())* exposure (); 
		}

		public Color CurrentGroundColor ()
		{
			return Ambient.GroundColor.Evaluate (normalizedTime ())* exposure (); 
		}
			
		[HideInInspector]
		float m_SunIntensity, m_MoonIntensity;

		void InitLightingParameters ()
		{
			m_SunIntensity = SunIntensity;
			m_MoonIntensity = MoonIntensity;
		}

		void SetLightingState ()
		{
			SetSunIntensity (SunIntensity);
			SetMoonIntensity (MoonIntensity);
		}

		void SetSunIntensity (float NewIntensity)
		{
			if (m_SunIntensity != NewIntensity) {
				m_SunIntensity = NewIntensity;
				uSkyInternal.MarkLightingStateDirty ();
			}
		}

		void SetMoonIntensity (float NewIntensity)
		{
			if (m_MoonIntensity != NewIntensity) {
				m_MoonIntensity = NewIntensity;
				uSkyInternal.MarkLightingStateDirty ();
			}
		}

		void SetAmbientMode ()
		{
			if(Ambient.useGradientMode)
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
		}

		public void OnValidate() 
		{
			UpdateDirectLighting (); // keep update the gradient color in Editor

			SetAmbientMode ();
		}

	}
}