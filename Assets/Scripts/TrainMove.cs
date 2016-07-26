using UnityEngine;
using System.Collections;
public class TrainMove : MonoBehaviour
{	
	void Start(){
		iTween.MoveBy(gameObject, iTween.Hash("x", 100, "easeType", "easeOutQuad", "time", 20, "delay", 20));
	}
}

