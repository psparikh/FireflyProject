using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("uSkyPro/Other/uSkyPro Script Access Example")]
public class uSkyScriptAccessExample : MonoBehaviour {

	// Custom parameter to control uSkyPro
	public float MyExposure = 1f;

	// Add more Custom parameter
	// .....

	uSkyPro uSP;
	
	void OnEnable () {
		// Initialize the uSkyPro
		uSP = uSkyPro.instance;
	}
	
	// Update is called once per frame
	void Update () {
		// Assign your custom parameter value to uSkyPro
		if (uSP != null) 
		{
			uSP.Exposure = MyExposure;
		// ..... others Custom parameter 

		}
	}
}
