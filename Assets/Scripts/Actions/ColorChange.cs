using UnityEngine;
using System.Collections;

public class ColorChange : MonoBehaviour {

    // Use this for initialization

    public ParticleSystem myParticle;
    public float waitTime = 2.0f;

    void Start() {

        print("Starting" + Time.time);
        StartCoroutine(WaitAndPrint(2.0f));


    }

    IEnumerator WaitAndPrint(float waitTime){

        yield return new WaitForSeconds(waitTime);
        print("WaitAndPrint " + Time.time);  
        myParticle.startColor = Color.red;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
