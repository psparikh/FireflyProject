using UnityEngine;
using System.Collections;

namespace uSky
{
	[AddComponentMenu("uSky/Update Probe Helper")]
	public class UpdateProbeHelper : MonoBehaviour {

		private int RenderId = -1;
		private ReflectionProbe TheProbe;

		PlayTOD playTOD;

		// Use this for initialization
		void Start () {
			TheProbe = GetComponent<ReflectionProbe>();

			TheProbe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
			TheProbe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;
			TheProbe.timeSlicingMode = UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.AllFacesAtOnce;

			StartCoroutine (ProcessProbesCoroutine());

			playTOD = PlayTOD.instance;

		}

		IEnumerator ProcessProbesCoroutine (){

			yield return null;
			RenderId = TheProbe.RenderProbe (null);	

//			Debug.Log ("Processing Probes Update Coroutine!");
		}

		void Update()
		{
			// if PlayTOD instance does not exist, switch refresh mode to every frame, then disable it-self
			if (playTOD == null) {
				TheProbe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame;
				enabled = false ;
			}
			else 
			{
				if (playTOD.UpdateProbes)
					StartCoroutine (ProcessProbesCoroutine ());

				if (TheProbe.IsFinishedRendering (RenderId)) {
					StopCoroutine (ProcessProbesCoroutine ());
					playTOD.UpdateProbes = false;

				}
			}
		}

	}
}