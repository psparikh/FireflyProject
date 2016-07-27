using UnityEngine;
using System.Collections;

public class LampController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Activate( false );
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Activate( bool toggle )
    {
        GetComponent<MeshRenderer>().enabled = !toggle;
        GetComponentInChildren<Light>().enabled = toggle;
    }
}
