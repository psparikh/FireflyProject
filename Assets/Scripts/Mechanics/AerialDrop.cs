using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AerialDrop : MonoBehaviour {

    public float spanSize;
    public int numDrops;
    public float dropRate;
    public Transform dropPoint;

    private float m_spanSize;
    private float m_numDrops;
    private float m_dropRate;

    private Vector3 minSpan;
    private Vector3 maxSpan;

    private Vector3[] dropSources;


    public GameObject[] spawnPrefabs;

	// Use this for initialization
	void Start () {

        dropSources = new Vector3[numDrops];

        DetermineDropPoints();
	
	}
	
	// Update is called once per frame
	void Update () {
        DetermineDropPoints();

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(minSpan, 0.5f);
        Gizmos.DrawSphere(maxSpan, 0.5f);

        for (int i = 0; i < numDrops; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(dropSources[i], 0.25f);

            Gizmos.color = Color.green;

            Vector3 to = new Vector3(dropSources[i].x, 0.0f, dropSources[i].z);

            Gizmos.DrawLine( dropSources[i], to);
        }
    }

    private void DetermineDropPoints()
    {
        minSpan = dropPoint.TransformPoint( -(spanSize/2.0f), 0.0f, 0.0f );
        maxSpan = dropPoint.TransformPoint(  (spanSize/2.0f), 0.0f, 0.0f);

        for( int i=0; i<numDrops; i++)
        {
            float offsetX = Mathf.Lerp(minSpan.x, maxSpan.x, i / (float)(numDrops - 1));
            float offsetY = Mathf.Lerp(minSpan.y, maxSpan.y, i / (float)(numDrops - 1));
            float offsetZ = Mathf.Lerp(minSpan.z, maxSpan.z, i / (float)(numDrops - 1));

            dropSources[i] = new Vector3( offsetX, offsetY, offsetZ );
        }

    }

    void FixedUpdate()
    {

       
    }
}
