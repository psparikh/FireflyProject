using UnityEngine;
using System.Collections;

public class NatureSpawn : MonoBehaviour {

    public GameObject natureResponsePrefab;
    private Vector3 positionOffset = new Vector3(2.533333f, 0.0f, 3.106668f);
    private Vector3 rotationOffset = new Vector3( 180.0f, 0.0f, 0.0f);
    private Vector3 scaleOffset = new Vector3(2.666667f, 2.666667f, 1.333334f)  ;


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
            GameObject natureResponse = (GameObject)Instantiate(natureResponsePrefab, transform.position, natureResponsePrefab.transform.rotation);
            natureResponse.transform.SetParent(transform);
            natureResponse.transform.localPosition = positionOffset;
            natureResponse.transform.localEulerAngles = rotationOffset;
            natureResponse.transform.localScale = scaleOffset;

            Destroy(natureResponse, 15.0f);

        }
    }


}
