using UnityEngine;
using System.Collections;

public class Break : MonoBehaviour
{

    void OnCollisionEnter(Collision col)
    {
        gameObject.AddComponent<TriangleExplosion>();
        StartCoroutine(gameObject.GetComponent<TriangleExplosion>().SplitMesh(true));
    }
}
