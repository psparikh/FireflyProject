using UnityEngine;

namespace usky
{
	// This script is helping to switch different "Shadow Distance" setting that based on the current scene.
	[ExecuteInEditMode]
	[AddComponentMenu("uSkyPro/Other/Shadow Distance Helper")]
	public class ShadowDistanceHelper : MonoBehaviour {

		[Tooltip("This is a shortcut to control the \"Shadow Distance\" in QualitySettings" +
			"\n\nNote: If this value sets to very high, then closeup objects shadow may become very blurly." +
		    "\n\nuSky does not require this script to run, this helper script can be removed at anytime.")]
		public float ShadowDistance = 150f;

		void Awake () 
		{
			QualitySettings.shadowDistance = ShadowDistance;
		}
		
		void OnValidate () 
		{
			ShadowDistance = Mathf.Max (ShadowDistance, 0f);
			Awake ();
		}
	}
}