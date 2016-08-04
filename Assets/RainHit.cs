using UnityEngine;
using System.Collections;

public class RainHit : MonoBehaviour {

    // Use this for initialization
    private ushort pulsePower = 700;
    public GameObject controller;
    private int count;

    void Start () {
        count = 0;
    }
	
	// Update is called once per frame
	void Update () {

        if (pulsePower > 0)
        {
            //controller.GetComponent<Haptic>().pulsePower = 700;
            count++;
        }

        if (count >= 20)
        {
            controller.GetComponent<Haptic>().pulsePower = 0;
            count = 0;
        }
    }

    void OnParticleCollision(GameObject other) {
        Debug.Log("here");

        controller.GetComponent<Haptic>().pulsePower = 500;
        if (other.CompareTag("rain"))
        {
            
        }
  
    }
}
