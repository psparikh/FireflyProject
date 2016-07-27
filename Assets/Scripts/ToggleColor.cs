using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ToggleColor : MonoBehaviour {

    public List<Color> colorArray;

    private Color defaultColor = Color.black;
    private int counter = 0;

	// Use this for initialization
	void Start () {

        Initialize();
	}
	
	// Update is called once per frame
	void OnCollisionEnter ( Collision col ) {
	
        if( col.collider.CompareTag("bullet") )
        {
            Increment();
        }

	}

    /// <summary>
    /// Initalize color array for toggling
    /// </summary>
    private void Initialize()
    {
        counter = 0;
        List<Color> originalArray = new List<Color> { GetComponent<Renderer>().material.color };

        if (colorArray.Count == 0)
        {
            originalArray.Add(defaultColor);
        }
        else
        {
            originalArray.AddRange(colorArray);
        }
        colorArray = originalArray;
    }

    /// <summary>
    /// toggle material color by incrementing counter
    /// </summary>
    public void Increment()
    {
        counter++;
        counter = counter % colorArray.Count;

        Renderer rend = GetComponent<Renderer>();
        rend.material.SetColor("_Color", colorArray[counter] );
    }
}
