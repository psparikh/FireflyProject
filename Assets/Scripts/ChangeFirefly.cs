using UnityEngine;
using System.Collections;

public class ChangeFirefly : MonoBehaviour {

    // Use this for initialization
    public ParticleSystem firefly;
    public SteamVR_TrackedObject controller;

	void Start () {

        firefly = gameObject.GetComponent<ParticleSystem>();

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void onTouchDown()
    {

    }
}
