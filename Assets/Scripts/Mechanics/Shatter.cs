using UnityEngine;
using System.Collections;

public class Shatter : MonoBehaviour
{

    void OnCollisionEnter(Collision col)
    {
        if (col.collider.tag == "bullet")
        {
            gameObject.AddComponent<TriangleExplosion>();
            StartCoroutine(gameObject.GetComponent<TriangleExplosion>().SplitMesh(true));
        }
    }

}