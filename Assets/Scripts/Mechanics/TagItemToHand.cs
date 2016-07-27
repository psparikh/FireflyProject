using UnityEngine;
using System.Collections;

public class TagItemToHand : MonoBehaviour {

    //public GameObject item;

    public GameObject slingshot;

	// Use this for initialization
	void Start () {
        slingshot.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {

        //slingshot.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        //slingshot.gameObject.transform.SetParent(gameObject.transform);

    }

    void OnTriggerEnter( Collider col )
    {
        if( col.name.StartsWith("Controller") )
        {
            slingshot.SetActive(true);
            slingshot.transform.position = transform.position;
        }
    }
}
