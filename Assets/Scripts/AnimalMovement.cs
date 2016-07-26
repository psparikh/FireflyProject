using UnityEngine;
using System.Collections;

public class AnimalMovement : MonoBehaviour {

    public GameObject animal;
    public Transform[] paths;

    private int currentPath = -1;

    void Awake()
    {
        currentPath = -1;
        foreach( Transform path in paths)
        {
            path.gameObject.SetActive(false);
        }
    }

	// Use this for initialization
	void Start () {
        NextPath();
    }

    void Update()
    {
    }



    void NextPath()
    {
        currentPath++;
        if (currentPath < paths.Length)
        {
            iTween.MoveTo(animal, iTween.Hash("path", paths[currentPath], "easeType", "easeInOutQuad", "time", 2, "delay", 1.0f, "oncomplete", "NextPath"));
        }
        else
        {
            iTween.MoveTo(animal, iTween.Hash("path", paths[currentPath], "easeType", "easeInOutQuad", "time", 2, "delay", 1.0f));
        }
    }

}
