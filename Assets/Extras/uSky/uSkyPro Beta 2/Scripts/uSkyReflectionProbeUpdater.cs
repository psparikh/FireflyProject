// The reflection update sequence controls by Event call.
//
// Usage : Apply this script to the Reflection Probe gameobject.

using UnityEngine;
using UnityEngine.Rendering;
using usky.Internal;

namespace usky
{
	[ExecuteInEditMode]
	[AddComponentMenu("uSkyPro/uSky ReflectionProbe Updater")]
	[DisallowMultipleComponent]
	public class uSkyReflectionProbeUpdater : MonoBehaviour {

		public ReflectionProbeTimeSlicingMode RuntimeRefreshMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;

		private ReflectionProbe TheProbe;

		void OnEnable (){
			TheProbe = GetComponent<ReflectionProbe> ();
			uSkyInternal.UpdateProbeEvent.AddListener	(RenderReflectionProbe);
		}
		void OnDisable (){
			uSkyInternal.UpdateProbeEvent.RemoveListener (RenderReflectionProbe);
		}

		void Start () {

			if (TheProbe == null) 
				enabled = false;
			
			TheProbe.mode = ReflectionProbeMode.Realtime;
			TheProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;

			if ( Application.isPlaying )
				TheProbe.timeSlicingMode = RuntimeRefreshMode;
			else
				TheProbe.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
			
//			if (uSkyPro.instance == null)
//				RenderReflectionProbe ();
		}

		// This function is called by UpdateProbeEvent
		void RenderReflectionProbe ()
		{
			TheProbe.RenderProbe ();
		}
	}
}