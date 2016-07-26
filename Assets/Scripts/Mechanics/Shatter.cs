using UnityEngine;
using System.Collections;

public class Shatter : MonoBehaviour
{

    void Start()
    {
        //gameObject.GetComponent<Rigidbody>().AddForce(0, 0, -200);
    }
    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "bullet")
        {
            gameObject.AddComponent<TriangleExplosion>();
            StartCoroutine(gameObject.GetComponent<TriangleExplosion>().SplitMesh(true));
        }
    }

}