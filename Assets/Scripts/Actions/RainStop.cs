﻿using UnityEngine;
using System.Collections;


public class RainStop : MonoBehaviour {

    // Use this for initialization
    public ParticleSystem system;
    public ParticleSystem explosionSystem;
    public float slowSpeed = 0.5f;
    public float lengthSpeed = 0.1f;
    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;
    

    void Start () {
        trackedObject = GetComponent<SteamVR_TrackedObject>();
    }
	
	void Update () {

        device = SteamVR_Controller.Input((int)trackedObject.index);

        if (device.GetPress(SteamVR_Controller.ButtonMask.Trigger))
        {

            Vector2 currentTriggerAxis = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
            float rainVel = Mathf.Lerp(-55f, -0.5f, currentTriggerAxis.x);
            ParticleSystem.VelocityOverLifetimeModule velLife = system.velocityOverLifetime;
            ParticleSystem.MinMaxCurve minMax = new ParticleSystem.MinMaxCurve();
            minMax.constantMax = rainVel;
            velLife.y = minMax;

            //float rainLength = Mathf.Lerp(0.5f,8f,)

            if (velLife.y.constantMax > -1)
            {
                explosionSystem.gravityModifier = 0; //FIX
                ParticleSystemRenderer pr = (ParticleSystemRenderer)system.GetComponent<ParticleSystemRenderer>();
                pr.renderMode = ParticleSystemRenderMode.Stretch;
                pr.lengthScale = 0.3f;

            }
            else
            {
                explosionSystem.gravityModifier = 5;
            }

            if (velLife.y.constantMax <= -45)
            {
                ParticleSystemRenderer pr = (ParticleSystemRenderer)system.GetComponent<ParticleSystemRenderer>();
                pr.renderMode = ParticleSystemRenderMode.Stretch;
                pr.lengthScale = 7;
            }

           else if (velLife.y.constantMax <= -35)
            {
                ParticleSystemRenderer pr = (ParticleSystemRenderer)system.GetComponent<ParticleSystemRenderer>();
                pr.renderMode = ParticleSystemRenderMode.Stretch;
                pr.lengthScale = 6;
            }
           else if (velLife.y.constantMax <= -25)
            {
                ParticleSystemRenderer pr = (ParticleSystemRenderer)system.GetComponent<ParticleSystemRenderer>();
                pr.renderMode = ParticleSystemRenderMode.Stretch;
                pr.lengthScale = 5;
            }
           else if (velLife.y.constantMax <= -10)
            {
                ParticleSystemRenderer pr = (ParticleSystemRenderer)system.GetComponent<ParticleSystemRenderer>();
                pr.renderMode = ParticleSystemRenderMode.Stretch;
                pr.lengthScale = 3;
            }
           else if (velLife.y.constantMax <= -5)
            {
                ParticleSystemRenderer pr = (ParticleSystemRenderer)system.GetComponent<ParticleSystemRenderer>();
                pr.renderMode = ParticleSystemRenderMode.Stretch;
                pr.lengthScale = 1.8f;
            }
            else if (velLife.y.constantMax <= -3)
            {
                ParticleSystemRenderer pr = (ParticleSystemRenderer)system.GetComponent<ParticleSystemRenderer>();
                pr.renderMode = ParticleSystemRenderMode.Stretch;
                pr.lengthScale = 1.5f;
            }
            else if (velLife.y.constantMax <= -1)
            {
                ParticleSystemRenderer pr = (ParticleSystemRenderer)system.GetComponent<ParticleSystemRenderer>();
                pr.renderMode = ParticleSystemRenderMode.Stretch;
                pr.lengthScale = 1.2f;
            }

        }

        //if button isn't pressed code so that it increases speed to -55

        if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
        {

            float rainVel = -55;
            ParticleSystem.VelocityOverLifetimeModule velLife = system.velocityOverLifetime;
            ParticleSystem.MinMaxCurve minMax = new ParticleSystem.MinMaxCurve();
            minMax.constantMax = rainVel;
            velLife.y = minMax;

            explosionSystem.gravityModifier = 5;

            ParticleSystemRenderer pr = (ParticleSystemRenderer)system.GetComponent<ParticleSystemRenderer>();
            pr.renderMode = ParticleSystemRenderMode.Stretch;
            pr.lengthScale = 8;

        }

    }

}    