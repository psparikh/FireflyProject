using UnityEngine;
using System.Collections;

public class TreeSpawn : MonoBehaviour {

	// Use this for initialization
	public void Grow () {

        iTween.ScaleBy(gameObject, iTween.Hash("x", 100,"y",100,"z",100, "easeType", "easeOutBounce", "time", 2));
    
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
