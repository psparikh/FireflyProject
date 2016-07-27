using UnityEngine;
using System.Collections;

public class iTweenMovementEvents : MonoBehaviour
{

    public iTweenMovement[] movements;
    private int currentMovement = 0;

    private bool initialized = false;
    private bool hasFinished = false;

    void Start()
    {
        currentMovement = 0;
        initialized = false;
        hasFinished = false;
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
        else
        {
            CallFinalEvent();
        }
    }

    private bool MovementsAreUpdated()
    {
        bool completeMovements = true;
        foreach (iTweenMovement tweenMove in GetComponentsInChildren<iTweenMovement>())
        {
            if (!tweenMove.IsPathInitialized())
            {
                completeMovements = false;
            }
        }
        return completeMovements;
    }

    void Update()
    {
        if( MovementsAreUpdated() && !initialized)
        {
            initialized = true;
        }
    }

    private void CallFinalEvent()
    {
        iTweenMovement finalMovement = movements[currentMovement];
        GameObject target = finalMovement.target;
        Transform finalWaypoint = finalMovement.waypoints[finalMovement.waypoints.Length - 1];

        target.transform.rotation = finalWaypoint.rotation;

        hasFinished = true;
    }

    public void Begin()
    {
        if ( initialized )
        {
            movements[currentMovement].iTweenPlay();
        }
    }

    public bool HasFinished()
    {
        return hasFinished;
    }

}
