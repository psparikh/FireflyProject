using UnityEngine;
using System.Collections;
public class TrainMove : MonoBehaviour
{
    public float rainTime;
    public GameObject rain;
    public ParticleSystem system;

    IEnumerator Start()
    {
        iTween.MoveBy(gameObject, iTween.Hash("z", -150, "easeType", "easeOutCubic", "time", 20, "delay", 80));

        if (rain.activeInHierarchy)
        {
            rain.SetActive(false);
        }

        Application.targetFrameRate = 100;

        yield return StartCoroutine(LateCall());
    }



    IEnumerator LateCall()
    {
        yield return new WaitForSeconds(rainTime);

        rain.SetActive(true);

        yield return new WaitForSeconds(1);

        ParticleSystem.EmissionModule emissionMod = system.emission;
        ParticleSystem.MinMaxCurve minMax = new ParticleSystem.MinMaxCurve();
        minMax.constantMax = 2000;
        emissionMod.rate = minMax;

    }
}

