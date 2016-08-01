using UnityEngine;
using usky.Internal;

namespace usky
{
	[ExecuteInEditMode]
	[AddComponentMenu("uSkyPro/uSky Sun")]
	public class uSkySun : MonoBehaviour {
		public static uSkySun instance;

		new public Transform	transform { get; private set; }
//		new public Light		light { get; private set; }

		void OnEnable() {
			if(instance) {
				Debug.LogErrorFormat("Not setting 'uSkySun.instance' because '{0}' is already active!", instance.name);
				return;
			}

			this.transform = base.transform;
//			this.light = GetComponent<Light>();
			instance = this;

		}

		void OnDisable() {
			if(instance == null) {
				Debug.LogErrorFormat("'uSkySun.instance' is already null when disabling '{0}'!", this.name);
				return;
			}
		
			if(instance != this) {
				Debug.LogErrorFormat("Not UNsetting 'uSkySun.instance' because it points to someone else '{0}'!", instance.name);
				return;
			}

			// Unity version 5.1.2 or newer
			GetComponent<Light>().RemoveAllCommandBuffers();

			instance = null;
		}

		// Takeover the events triggering if detected No uSkyTimeline or uSkyPro instance in current scene
		void Update ()
		{
//			Debug.Log ("Sun CommendBuffer Count :  " + GetComponent<Light>().commandBufferCount );

			if (uSkyTimeline.instance != null && uSkyPro.instance != null)
				return;
			else
				CheckSunTransformState ();
		}

		void CheckSunTransformState ()
		{
			if (this.transform.hasChanged && instance == this) 
			{
				uSkyInternal.MarkLightingStateDirty ();
				uSkyInternal.MarkAtmosphereStateDirty ();

				if (uSkyPro.instance == null)
					uSkyInternal.MarkProbeStateDirty ();
				
				this.transform.hasChanged = false;
			}
		}
	}
}