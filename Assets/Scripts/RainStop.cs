using UnityEngine;
using System.Collections;


public class RainStop : MonoBehaviour {

    // Use this for initialization
    public ParticleSystem system;
    public ParticleSystem explosionSystem;
    public float slowSpeed = 0.5f;
    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;

    void Start () {
        trackedObject = GetComponent<SteamVR_TrackedObject>();

        ParticleSystem.EmissionModule emissionMod = system.emission;
        ParticleSystem.MinMaxCurve minMax = new ParticleSystem.MinMaxCurve();
        minMax.constantMax = 2000;
        emissionMod.rate = minMax;


    }
	
	// Update is called once per frame
	void Update () {

        device = SteamVR_Controller.Input((int)trackedObject.index);

        if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
        {
            ParticleSystem.VelocityOverLifetimeModule velLife = system.velocityOverLifetime;
            ParticleSystem.MinMaxCurve minMax = new ParticleSystem.MinMaxCurve();
            minMax = velLife.y;
            minMax.constantMax = velLife.y.constantMax + slowSpeed;

            if (velLife.y.constantMax > -1)
            {
                explosionSystem.gravityModifier = 0; //FIX

            }
            else
            {
                explosionSystem.gravityModifier = 4;

            }

            if (velLife.y.constantMax <= -1)
            {
                velLife.y = minMax;
                Debug.Log(velLife.y.constantMax);
            }



        }

        //if button isn't pressed code so that it increases speed to -55

        if (device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            Debug.Log("g");
            ParticleSystem.VelocityOverLifetimeModule velLife = system.velocityOverLifetime;
            ParticleSystem.MinMaxCurve minMax = new ParticleSystem.MinMaxCurve();
            minMax = velLife.y;
            Debug.Log(minMax.constantMax);
            Debug.Log(velLife.y.constantMax);
            minMax.constantMax = -55;
            Debug.Log(minMax.constantMax);
            velLife.y = minMax;
            

        }

    }

}    


/*
    void InitializeIfNeeded()
    {
        if (system == null) {
            system = GetComponent<ParticleSystem>();
        }

        if (particles == null || particles.Length < system.maxParticles)
        {
            particles = new ParticleSystem.Particle[system.maxParticles];
        }

    }*/
            /* int numParticlesAlive = system.GetParticles(particles);
             for (int i = 0; i < numParticlesAlive; i++)
             {

                // if (particles[i].velocity != Vector3.zero)
                // {
                     particles[i].velocity = Vector3.zero;

                // }
             }*/