using UnityEngine;
using System.Collections;
public class TrainMove : MonoBehaviour
{	
	void Start(){
		iTween.MoveBy(gameObject, iTween.Hash("z", -150, "easeType", "easeOutCubic", "time", 20, "delay", 0));
	}
}

