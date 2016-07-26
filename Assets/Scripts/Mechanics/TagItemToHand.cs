using UnityEngine;
using System.Collections;

public class TagItemToHand : MonoBehaviour {

    public GameObject item;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter( Collider col)
    {
        if( col.name.StartsWith("Controller") )
        {
            Destroy(item);
        }
    }
}
