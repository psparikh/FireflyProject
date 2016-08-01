using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[AddComponentMenu("uSky/uFog Gradient (Stardard Fog)")]
public class uFogGradient : MonoBehaviour {

	public Gradient FogColor = new Gradient()
	{
		colorKeys = new GradientColorKey[] {
			new GradientColorKey(new Color32(19, 32, 45, 255), 0.22f),
			new GradientColorKey(new Color32(189, 148, 62, 255), 0.25f),
			new GradientColorKey(new Color32(223, 246, 252, 255), 0.27f),
			new GradientColorKey(new Color32(223, 246, 252, 255), 0.73f),
			new GradientColorKey(new Color32(189, 148, 62, 255), 0.75f),
			new GradientColorKey(new Color32(19, 32, 45, 255), 0.78f)
		},
		alphaKeys = new GradientAlphaKey[] {
			new GradientAlphaKey(1.0f, 0.0f),
			new GradientAlphaKey(1.0f, 1.0f)
		}
	};

	private uSkyManager uSM;
	
	// Use this for initialization
	void Start () {
		uSM = uSkyManager.instance;
		RenderSettings.fogColor = currentFogColor ();
	}
	
	// Update is called once per frame
	void Update () {
		if (uSM != null )
			if (uSM.SkyUpdate)
				RenderSettings.fogColor = currentFogColor ();
	}

	Color currentFogColor (){
		float currentTime = (uSM != null )? uSM.Timeline01 : 1f;
		return FogColor.Evaluate (currentTime);
	}
}
