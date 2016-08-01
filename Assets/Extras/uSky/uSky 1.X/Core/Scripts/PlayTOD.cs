using UnityEngine;
using System.Collections;

namespace uSky
{
	[AddComponentMenu("uSky/Play TOD")]
	public class PlayTOD : MonoBehaviour {

		public static PlayTOD instance { get; private set; }

		public bool PlayTimelapse = true;
		
		[Tooltip("Controls how fast the sun moves between Interval update.")]
		public float PlaySpeed = 0.005f;
		
		[Tooltip ("Controls how fast the play speed during day and night cycles.\n\nDefault time curve keys have been set to 25% faster at night." +
		          "\n\nCurve key value of 1 means no speed change, and if key value is higher means the play speed is faster, vice versa.")]
		public AnimationCurve TimeSpeedCurve = new AnimationCurve(new Keyframe(0f, 1.25f),new Keyframe(5f, 1.25f), new Keyframe(6.5f, 1f),
		                                                          new Keyframe(17.5f, 1f), new Keyframe(19f, 1.25f), new Keyframe(24f, 1.25f));
		
		[Tooltip ("Update Timeline per second.")]
		public float steppedInterval = 10f;
		float accumulatedTime;
		public float actualTime;

		private uSkyManager uSM;

		public bool UpdateProbes { get; set;}

		void OnEnable() {

			if(instance && instance != this)
				Debug.LogErrorFormat("Unexpected: PlayTOD.instance already set (to: {0}). Still overriding with: {1}.", instance.name, name);
			
			instance = this;
		}

		void Start (){
			uSM = uSkyManager.instance;
			
			if (PlayTimelapse)
				uSM.SkyUpdate = true;
			
			actualTime = uSM.Timeline;
		}

		void Update()
		{
			if (PlayTimelapse) 
			{
				accumulatedTime += Time.deltaTime;
				
				actualTime += Time.deltaTime * PlaySpeed * TimeSpeedCurve.Evaluate(actualTime);
				
				if (actualTime > 24)
					actualTime = 0;
				
				if (accumulatedTime >= steppedInterval)
				{
					uSM.Timeline = actualTime;
					accumulatedTime = 0f;

					UpdateProbes = true;
				}
			}
		}

		void OnDisable (){
			instance = null;
		}

		public void OnValidate() {
			PlaySpeed = Mathf.Max (PlaySpeed, 0f);
			steppedInterval = Mathf.Max (steppedInterval, 0.3f);
//			if (enabled && base.isActiveAndEnabled) {
//				OnDisable ();
//				OnEnable ();
//			}
		}
	}
}