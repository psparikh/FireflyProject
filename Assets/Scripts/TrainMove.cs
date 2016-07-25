using UnityEngine;
using System.Collections;
public class TrainMove : MonoBehaviour
{	
	void Start(){
		iTween.MoveBy(gameObject, iTween.Hash("z", -100, "easeType", "easeOutQuad", "time", 5, "delay", .1));
	}
}

