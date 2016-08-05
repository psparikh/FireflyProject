using UnityEngine;
using System.Collections;
using SWS;

public class DragonController : MonoBehaviour {

    private splineMove dragonSpline;
    private AerialDrop aerialDrop;

	// Use this for initialization
	void Start () {
        dragonSpline = GetComponent<splineMove>();
        aerialDrop = GetComponent<AerialDrop>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}


}
