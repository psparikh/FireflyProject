using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

    public GameObject explosionParticlesPrefab;

    public void OnCollisionEnter(Collision collision)
    {
        if (explosionParticlesPrefab)
        {
            GameObject explosion = (GameObject)Instantiate(explosionParticlesPrefab, transform.position, explosionParticlesPrefab.transform.rotation);
            Destroy(explosion, explosion.GetComponent<ParticleSystem>().startLifetime);
        }
        Destroy(gameObject);
    }
}
