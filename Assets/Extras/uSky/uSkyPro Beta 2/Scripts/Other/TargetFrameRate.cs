using UnityEngine;

[AddComponentMenu("uSkyPro/Other/Target Frame Rate")]
public class TargetFrameRate : MonoBehaviour {

	public int FrameRate = 60;

	// Use this for initialization
	void Awake () {
		Application.targetFrameRate = FrameRate;
	}

}
