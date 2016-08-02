using UnityEngine;
using System.Collections;

public class TrainMoveCinema : MonoBehaviour {

    public ParticleSystem system;

    public void Move()
    {
        iTween.MoveBy(gameObject, iTween.Hash("z", -150, "easeType", "easeOutCubic", "time", 20, "delay", 0));
    }



    public void StartRain()
    {

        ParticleSystem.EmissionModule emissionMod = system.emission;
        ParticleSystem.MinMaxCurve minMax = new ParticleSystem.MinMaxCurve();
        minMax.constantMax = 2000;
        emissionMod.rate = minMax;

    }
}
