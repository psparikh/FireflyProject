using UnityEngine;
using System.Collections;

public class NatureSpawn : MonoBehaviour {

    public GameObject natureResponsePrefab;
    public Transform natureResponsePoint;
    public float timeElapsed = 15.0f;

	// Use this for initialization
	void Start () {
        Grow();
    }

    // Update is called once per frame
    void Update () {
	
	}

    public void OnCollisionEnter(Collision col)
    {
        if (col.collider.CompareTag("bullet"))
        {
            SpawnResponse();
        }
    }

    public void Grow()
    {
        iTween.ScaleBy(gameObject, iTween.Hash("x", 10, "y", 10, "z", 10, "easeType", "easeOutBounce", "time", 2));
    }

    /// <summary>
    /// Response on Hit
    /// </summary>
    public void SpawnResponse()
    {
        if (natureResponsePrefab)
        {
            GameObject natureResponse = (GameObject)Instantiate(natureResponsePrefab, natureResponsePoint.position, natureResponsePoint.rotation);
            Destroy(natureResponse, timeElapsed);

        }
    }


}
