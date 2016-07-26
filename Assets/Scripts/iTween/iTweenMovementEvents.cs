using UnityEngine;
using System.Collections;

public class iTweenMovementEvents : MonoBehaviour
{

    public iTweenMovement[] movements;
    private int currentMovement = 0;

    void Start()
    {
        currentMovement = 0;
        movements[currentMovement].iTweenPlay();
    }

    public void OnStartEvent()
    {
    }

    public void OnCompleteEvent()
    {

        if (currentMovement < movements.Length-1)
        {
            currentMovement++;
            movements[currentMovement].iTweenPlay();
        }
    }

}
