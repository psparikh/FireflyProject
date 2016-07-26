using UnityEngine;
using System.Collections;

public class AnimalMove : MonoBehaviour {

    private Vector3 startPos;
    private Vector3 endPos;

    public GameObject target;

    void Awake()
    {
        startPos = gameObject.transform.position;
        endPos = target.transform.position;
    }

	// Use this for initialization
	void Start () {
        iTween.MoveTo(gameObject, iTween.Hash("position", endPos, "easeType", "easeInOutExpo", "loopType", "none", "delay", 1.0f));
    }

}
