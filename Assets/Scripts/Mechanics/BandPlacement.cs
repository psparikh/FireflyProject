using UnityEngine;
using System.Collections;

public class BandPlacement : MonoBehaviour {

    public Transform startPoint;
    public Transform endPoint;

    private LineRenderer line;

	// Use this for initialization
	void Start () {
        line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update () {
        line.SetPosition(0, startPoint.position);
        line.SetPosition(1, endPoint.position);

    }
}
