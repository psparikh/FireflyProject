using UnityEngine;
using System.Collections;

public class AnimalManager : MonoBehaviour {

    public GameObject animalMovementHolder;
    private iTweenMovementEvents movement;

    private bool hasActivated;

    void Awake()
    {

    }

	// Use this for initialization
	void Start () {
        movement = animalMovementHolder.GetComponent<iTweenMovementEvents>();
    }

    void Update()
    {
        if (!hasActivated && movement.HasInitialized() )
        {
            movement.Begin();
            hasActivated = true;
        }
    }
}
