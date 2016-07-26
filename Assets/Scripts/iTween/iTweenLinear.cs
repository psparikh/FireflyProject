using UnityEngine;
using System.Collections;

public class iTweenLinear : MonoBehaviour {

    public Transform[] waypointArray;

	// Use this for initialization
	void Start () {
        iTween.MoveTo(gameObject, iTween.Hash("path", waypointArray, "easeType", "linearTween", "time", 2, "delay", 1.0f));
    }

    void Update()
    {
    }

    void OnDrawGizmos()
    {
        //Visual. Not used in movement
        iTween.DrawPath(waypointArray);
    }

}
