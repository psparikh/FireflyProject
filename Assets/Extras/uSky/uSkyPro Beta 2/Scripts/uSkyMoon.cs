using UnityEngine;
using usky.Internal;

namespace usky
{
	[ExecuteInEditMode]
	[AddComponentMenu("uSkyPro/uSky Moon")]
	public class uSkyMoon : MonoBehaviour {
		public static uSkyMoon instance;

		new public Transform	transform { get; private set; }
//		new public Light		light { get; private set; }
		
		void OnEnable() {
			if(instance) {
				Debug.LogErrorFormat("Not setting 'uSkyMoon.instance' because '{0}' is already active!", instance.name);
				return;
			}

			this.transform = base.transform;
//			this.light = GetComponent<Light>();
			instance = this;
		}

		void OnDisable() {
			if(instance == null) {
				Debug.LogErrorFormat("'uSkyMoon.instance' is already null when disabling '{0}'!", this.name);
				return;
			}
		
			if(instance != this) {
				Debug.LogErrorFormat("Not UNsetting 'uSkyMoon.instance' because it points to someone else '{0}'!", instance.name);
				return;
			}

			// Unity version 5.1.2 or newer
			GetComponent<Light>().RemoveAllCommandBuffers();

			instance = null;
		}

		// Takeover the events triggering if detected no uSkyTimeline instance in current scene
		void Update ()
		{
			if (uSkyTimeline.instance != null)
				return;
			else
				CheckMoonTransformState ();
		}

		void CheckMoonTransformState ()
		{
			if (this.transform.hasChanged && instance == this) {
				uSkyInternal.MarkLightingStateDirty ();
				uSkyInternal.MarkAtmosphereStateDirty ();
				this.transform.hasChanged = false;
			}
		}
	}
}
