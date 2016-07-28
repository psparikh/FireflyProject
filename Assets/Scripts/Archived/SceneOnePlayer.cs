/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

 /* @author: Jewel Lim 
 * Scene 1: Player mechanics
 * /

    public class SceneOnePlayer : MonoBehaviour {

    public Light playerCellPhoneLight;
    public Light humanoidCellPhoneLight;
    public GameObject humanoid;
    public ParticleSystem leavesParticle;
    public SteamVR_TrackedObject controller;
    public GameObject trainBox;
    private GameObject selectedObject;

    // Use this for initialization
    void Start () {

        humanoidCellPhoneLight.AddComponent<Light>();
        leavesParticle = gameObject.GetComponent<ParticleSystem>();
        leavesParticle.enabled = false;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        humanoidCellPhoneLight.enabled = false; //Select an object by touching it
        Destroy(humanoid);
        leavesParticle.enabled = false;
    
    }

    void OnTriggerExit(Collider other)
    {
        Destroy(trainBox);
    }
} 
*/



