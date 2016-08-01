using UnityEngine;
using UnityEngine.Rendering;
using usky.Internal;

namespace usky
{
	[ExecuteInEditMode]
	[AddComponentMenu("uSkyPro/uSky Fog Gradient (Standard Fog)")]
	public class uSkyFogGradient : MonoBehaviour
	{
		[Tooltip("Control unity built-in fog color in Lighting window")]
		public Gradient FogColor = new Gradient()
		{
			colorKeys = new GradientColorKey[] {
				new GradientColorKey( new Color32(19, 32, 45, 255), 0.48f),
				new GradientColorKey( new Color32(240, 162, 66, 255), 0.52f),
				new GradientColorKey( new Color32(217, 200, 119, 255), 0.53f),
				new GradientColorKey( new Color32(204, 226, 232, 255), 0.65f),
				new GradientColorKey( new Color32(161, 194, 233, 255), 1.0f)
			},
			alphaKeys = new GradientAlphaKey[] {
				new GradientAlphaKey(1.0f, 0.0f),
				new GradientAlphaKey(1.0f, 1.0f)
			}
		};
							
		void OnEnable ()
		{
			RenderSettings.fog = true;
			uSkyInternal.UpdateAtmosphereEvent.AddListener (UpdateUnityFog);
		}

		void OnDisable ()
		{
			RenderSettings.fog = false;
			uSkyInternal.UpdateAtmosphereEvent.RemoveListener (UpdateUnityFog);
		}

		void Start ()
		{
			UpdateUnityFog ();
		}
		
		// Update is called by UpdateAtmosphereEvent
		void UpdateUnityFog ()
		{
			float t = uSkyInternal.NormalizedTime (uSkySun.instance, uSkyMoon.instance);
			RenderSettings.fogColor = FogColor.Evaluate ( t );
		
		}
			
	}
}