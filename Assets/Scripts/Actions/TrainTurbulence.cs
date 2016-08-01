using UnityEngine;
using System.Collections;
using SWS;
using UnityStandardAssets.Utility;

public class TrainTurbulence : MonoBehaviour {

    public splineMove handle;
    private PathManager trainMotion;

    public float angleRange;
    private Vector3 originalRotation;

	// Use this for initialization
	void Start () {
        trainMotion = handle.pathContainer;
        originalRotation = transform.localEulerAngles;
	}
	
	// Update is called once per frame
	void Update () {

        if( handle != null)
        {
            SimulateTurbulence();
        }

    }

    /// <summary>
    /// Simulate train turbulence by interpolating handle
    /// </summary>
    private void SimulateTurbulence()
    {
        Transform min = trainMotion.waypoints[0];
        Transform max = trainMotion.waypoints[trainMotion.waypoints.Length - 1];

        float percentage = Arithmetic.Normalize(handle.transform.position.x, min.position.x, max.position.x);
        float rotAngle = Arithmetic.PercentToRange(percentage, angleRange, true);

        transform.localEulerAngles = new Vector3(originalRotation.x, originalRotation.y, rotAngle);
    }
}
