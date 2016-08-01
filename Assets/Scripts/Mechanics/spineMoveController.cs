using UnityEngine;
using System.Collections;
using SWS;


public class spineMoveController : MonoBehaviour {

    public splineMove source;

    private bool isPaused;
    public bool pause;

	// Use this for initialization
	void Start () {
        if( !source )
            source = GetComponent<splineMove>();
	}
	
	// Update is called once per frame
	void Update () {

        if( pause != isPaused )
        {
            if (pause) source.Pause();
            else source.Resume();

            isPaused = pause;
        }

	
	}

    
}
