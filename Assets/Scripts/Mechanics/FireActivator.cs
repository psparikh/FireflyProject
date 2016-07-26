using UnityEngine;
using System.Collections;
using UnityStandardAssets.Effects;

[RequireComponent(typeof(ExtinguishableParticleSystem))]
public class FireActivator : MonoBehaviour {

    private bool toggleFire = false;

	// Use this for initialization
	void Start () {
        toggleFire = false;
        GetComponent<ExtinguishableParticleSystem>().Extinguish(!toggleFire);
    }
	
	// Update is called once per frame
	void Update () {
        if ( Input.GetKeyDown(KeyCode.F) )
        {
            toggleFire = !toggleFire;
            Debug.Log(toggleFire);

            GetComponent<ExtinguishableParticleSystem>().Extinguish(!toggleFire);
        }
    }

    public void OnTriggerEnter( Collider col)
    {
        if( col.CompareTag("bullet") )
        {
            toggleFire = !toggleFire;
            GetComponent<ExtinguishableParticleSystem>().Extinguish(!toggleFire);
        }
    }
}
