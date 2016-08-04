using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

    public GameObject explosionParticlesPrefab;
    public GameObject spawnedItem;

    public void OnCollisionEnter(Collision collision)
    {
        if (explosionParticlesPrefab)
        {
            GameObject explosion = (GameObject)Instantiate(explosionParticlesPrefab, transform.position, explosionParticlesPrefab.transform.rotation);
            Destroy(explosion, explosion.GetComponent<ParticleSystem>().startLifetime);
        }


        if(collision.collider.CompareTag("terrain"))
        {
            Vector3 contactPoint = collision.contacts[0].point;
            GameObject newItem = (GameObject)Instantiate(spawnedItem, contactPoint, Quaternion.identity);
        }

        Destroy(gameObject);

    }
}
