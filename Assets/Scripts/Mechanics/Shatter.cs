using UnityEngine;
using System.Collections;

public class Shatter : MonoBehaviour
{

    private float s_radius = 0.37f;
    private float s_power = 100.0f;
    private float s_upwardsModifier = 3.0f;

    public float multiplier = 1.0f;

    void OnCollisionEnter(Collision col)
    {
        if (col.collider.tag == "bullet" )
        {
            Vector3 contactPoint = col.contacts[0].point;
            Collider[] colliders = Physics.OverlapSphere(contactPoint, s_radius * multiplier, ~(LayerMask.NameToLayer("triangle")));

            foreach (Collider hit in colliders)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.transform.SetParent(null);

                    rb.useGravity = true;
                    rb.isKinematic = false;

                    Vector3 pos = new Vector3(contactPoint.x, contactPoint.y, contactPoint.z);

                    rb.AddExplosionForce(s_power, pos, s_radius * multiplier, s_upwardsModifier);
                    Destroy(rb.gameObject, Random.Range(3.0f, 5.0f));
                }

            }
        }
    }

}