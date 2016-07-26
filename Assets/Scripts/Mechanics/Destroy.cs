using UnityEngine;
using System.Collections;

public class Destroy : MonoBehaviour
{

    public bool triggerEvent = false;

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
            //col.gameObject.GetComponent<Collide>().device.TriggerHapticPulse(700);
        }
    }

    void Update()
    {
        if (triggerEvent)
        {
            gameObject.AddComponent<TriangleExplosion>();
            StartCoroutine(gameObject.GetComponent<TriangleExplosion>().SplitMesh(true));
            //col.gameObject.GetComponent<Collide>().device.TriggerHapticPulse(700);
            triggerEvent = false;
        }
    }
}