using UnityEngine;
using System.Collections;

public class AnimalMove : MonoBehaviour {

    private Vector3 startPos;
    private Vector3 endPos;

    public Transform[] waypointArray;
    float percentsPerSecond = 0.10f; // %2 of the path moved per second
    float currentPathPercent = 0.0f; //min 0, max 1

    //public GameObject target;

    void Awake()
    {
        startPos = gameObject.transform.position;
        //endPos = target.transform.position;
    }

	// Use this for initialization
	void Start () {
        //iTween.MoveTo(gameObject, iTween.Hash("position", endPos, "easeType", "easeInOutExpo", "loopType", "none", "delay", 1.0f));

        iTween.MoveTo(gameObject, iTween.Hash("path", waypointArray, "easeType", "easeInOutQuad", "time", 2));
    }

    void Update()
    {
        //currentPathPercent += percentsPerSecond * Time.deltaTime;
        //iTween.PutOnPath(gameObject, waypointArray, currentPathPercent);
    }

    void OnDrawGizmos()
    {
        //Visual. Not used in movement
        iTween.DrawPath(waypointArray);
    }

}
