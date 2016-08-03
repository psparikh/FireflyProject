using UnityEngine;
using System.Collections;

public class Shatter : MonoBehaviour
{

    public float radius = 1.5f;
    public float power = 200.0f;
    public float upwardsModifier = 20.0f;

    void OnCollisionEnter(Collision col)
    {

        if (col.collider.tag == "bullet" )
        {
            Vector3 contactPoint = col.contacts[0].point;
            Collider[] colliders = Physics.OverlapSphere(contactPoint, radius, ~(LayerMask.NameToLayer("triangle")));

            foreach (Collider hit in colliders)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.transform.SetParent(null);

                    rb.useGravity = true;
                    rb.isKinematic = false;

                    Vector3 pos = new Vector3(contactPoint.x + Random.Range(-0.5f, 0.5f), contactPoint.y + Random.Range(-0.5f, 0.5f), contactPoint.z + Random.Range(-0.5f, 0.5f));

                    rb.AddExplosionForce(power, pos, radius, upwardsModifier);
                    Destroy(rb.gameObject, Random.Range(3.0f, 5.0f));
                }

            }
        }
    }

}