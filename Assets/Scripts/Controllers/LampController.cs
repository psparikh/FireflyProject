using UnityEngine;
using System.Collections;

public class LampController : MonoBehaviour {

    public bool hasActivated;
    public bool canToggle = true;

	// Use this for initialization
	void Start () {
        hasActivated = false;
        Activate( false );
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Activate( bool toggle )
    {
        if (canToggle)
        {
            GetComponent<MeshRenderer>().enabled = !toggle;
            GetComponentInChildren<Light>().enabled = toggle;
            hasActivated = toggle;
        }
    }
}
