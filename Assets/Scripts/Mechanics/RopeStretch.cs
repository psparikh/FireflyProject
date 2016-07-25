using UnityEngine;
using System.Collections;

public class RopeStretch : MonoBehaviour {

    public Transform startPoint;
    public Transform endPoint;

    private float width;

	// Use this for initialization
	void Start () {
        width = transform.localScale.x;
	}
	
	// Update is called once per frame
	void Update () {

        Vector3 offset = endPoint.position - startPoint.position;
        Vector3 scale = new Vector3(width, offset.magnitude / 2.0f, width);
        Vector3 position = startPoint.position + (offset / 2.0f);

        transform.position = position;
        transform.up = offset;
        transform.localScale = scale;
        
	}
}
