﻿using UnityEngine;
using System.Collections;

public class ChangeFirefly : MonoBehaviour {

    // Use this for initialization
    public ParticleSystem firefly;
    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;

	void Start () {
        trackedObject = GetComponent<SteamVR_TrackedObject>();
        firefly.Stop();
	}
	
	// Update is called once per frame
	void Update () {
        device = SteamVR_Controller.Input((int)trackedObject.index);

        if (device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            firefly.Play();
            ParticleSystem.EmissionModule emissionMod = firefly.emission;
            ParticleSystem.MinMaxCurve minMax = new ParticleSystem.MinMaxCurve();
            minMax.constantMax = 0;
            emissionMod.rate = minMax;
        }
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            firefly.Play();
            ParticleSystem.EmissionModule emissionMod = firefly.emission;
            ParticleSystem.MinMaxCurve minMax = new ParticleSystem.MinMaxCurve();
            minMax.constantMax = 100;
            emissionMod.rate = minMax;

        }

    }
}
