using UnityEngine;
using System.Collections;
using UnityStandardAssets.Effects;

[RequireComponent(typeof(ExtinguishableParticleSystem))]
public class FireActivator : MonoBehaviour {

    private bool active = false;
    private bool initilaized = false;

	// Use this for initialization
	void Start () {

        if (!initilaized)
        {
            ParticleSystem[] m_Systems = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem system in m_Systems)
            {
                system.Play();
            }
            initilaized = true;
        }

    }

    public void OnCollisionEnter( Collision col)
    {
        if( col.collider.CompareTag("bullet") )
        {
            active = !active;
            GetComponent<ExtinguishableParticleSystem>().Extinguish(active);
        }
    }
}
