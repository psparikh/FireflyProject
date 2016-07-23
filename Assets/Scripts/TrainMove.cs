using UnityEngine;
using System.Collections;
<<<<<<< HEAD

public class TrainMove : MonoBehaviour
{	
	void Start(){
		iTween.MoveBy(gameObject, iTween.Hash("z", -100, "easeType", "easeOutQuad", "time", 5, "delay", .1));
	}
=======
//Priyam
public class TrainMove : MonoBehaviour
{	
	void Start(){
		iTween.MoveBy(gameObject, iTween.Hash("z", -100, "easeType", "easeOutQuart", "time", 10, "delay", .1));
	}

    //using a iTween Animation Tool that will move the gameObject according to a curve - gives a realistic train movement
>>>>>>> ca31c4424654d901f72a7626454fb015e3c3a4aa
}

