using UnityEngine;
using System.Collections;

public class HumanoidsManager : MonoBehaviour {

    public GameObject humanoidMovementHolder;
    private bool endMovement = false;

    public GameObject lampHolder;
    private bool activateHumanoids;

    private LampController[] lampControllers;

	// Use this for initialization
	void Start () {
        activateHumanoids = false;
        lampControllers = lampHolder.GetComponentsInChildren<LampController>();
	}
	
	// Update is called once per frame
	void Update () {

        if( !activateHumanoids)
        {
            bool allLampsActivated = true;
            foreach( LampController lampControl in lampControllers)
            {
                if( !lampControl.hasActivated)
                {
                    allLampsActivated = false;
                }
            }

            if (allLampsActivated)
            {
                ActivateHumanoids();
                activateHumanoids = true;
            }
        }

        else
        {
            if( HumanoidsFinished() && !endMovement )
            {
                CallEndMovement();
                endMovement = true;
            }
        }

	}

    private void ActivateHumanoids()
    {
        iTweenMovementEvents[] movements = humanoidMovementHolder.GetComponentsInChildren<iTweenMovementEvents>();
        for( int i=0; i<movements.Length; i++)
        {
            movements[i].Invoke("Begin", (float)i);
        }
    }

    private bool HumanoidsFinished()
    {
        bool finished = true;
        iTweenMovementEvents[] movements = humanoidMovementHolder.GetComponentsInChildren<iTweenMovementEvents>();
        for (int i = 0; i < movements.Length; i++)
        {
            if (!movements[i].HasFinished())
            {
                finished = false;
            }
        }
        return finished;
    }

    private void CallEndMovement()
    {
        foreach (LampController lampControl in lampControllers)
        {
            lampControl.Activate(false);
            lampControl.canToggle = false;
        }
    }
}
