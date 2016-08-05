using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AerialDrop : MonoBehaviour {

    public float spanSize;
    public int numDrops;
    public float dropRate;

    private float m_spanSize;
    private int m_numDrops;
    private float m_dropRate;

    public Transform dropPoint;

    private Vector3 minSpan;
    private Vector3 maxSpan;
    private Vector3[] dropSources;

    public GameObject[] spawnPrefabs;

    private float nextDrop;

    private float nextFire;

    public float spanSizeRange;
    public int numDropsRange;
    public float dropRateRange;

    public bool doDrop = false;

    // Use this for initialization
    void Start () {

        RandomizeParams();
        DetermineDropPoints();	
	}
	
	// Update is called once per frame
	void Update () {

        if (doDrop)
        {
            RandomizeParams();
            DetermineDropPoints();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(minSpan, 0.5f);
        Gizmos.DrawSphere(maxSpan, 0.5f);

        for (int i = 0; i < m_numDrops; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(dropSources[i], 0.25f);

            Gizmos.color = Color.green;
            Vector3 to = new Vector3(dropSources[i].x, 0.0f, dropSources[i].z);
            Gizmos.DrawLine( dropSources[i], to);
        }
    }

    void FixedUpdate()
    {
        if (doDrop)
        {
            SpawnItems();
        }
    }

    /// <summary>
    /// Determine Drop Points
    /// </summary>
    private void DetermineDropPoints()
    {
        dropSources = new Vector3[m_numDrops];

        minSpan = dropPoint.TransformPoint( -(m_spanSize/2.0f), 0.0f, 0.0f );
        maxSpan = dropPoint.TransformPoint(  (m_spanSize/2.0f), 0.0f, 0.0f);

        for( int i=0; i<m_numDrops; i++)
        {
            float offsetX = Mathf.Lerp(minSpan.x, maxSpan.x, i / (float)(m_numDrops - 1));
            float offsetY = Mathf.Lerp(minSpan.y, maxSpan.y, i / (float)(m_numDrops - 1));
            float offsetZ = Mathf.Lerp(minSpan.z, maxSpan.z, i / (float)(m_numDrops - 1));

            dropSources[i] = new Vector3( offsetX, offsetY, offsetZ );
        }

    }

    /// <summary>
    /// Randomize Parameters
    /// </summary>
    public void RandomizeParams()
    {
        m_spanSize = spanSize + Mathf.Lerp(-(spanSizeRange / 2.0f), (spanSizeRange / 2.0f), Random.value);
        m_numDrops = numDrops + (int)Mathf.Lerp(-(numDropsRange / 2.0f), (numDropsRange / 2.0f), Random.value);
        m_dropRate = dropRate + Mathf.Lerp(-(dropRateRange / 2.0f), (dropRateRange / 2.0f), Random.value);
    }


    /// <summary>
    /// Spawn items
    /// </summary>
    void SpawnItems()
    {
        if (Time.time > nextDrop)
        {
            nextDrop = Time.time + m_dropRate;

            for (int i = 0; i < m_numDrops; i++)
            {
                RaycastHit hit;

                if (Physics.Raycast(dropSources[i], -Vector3.up, out hit))
                {
                    if (hit.collider.CompareTag("terrain"))
                    {
                        GameObject item = (GameObject)Instantiate(spawnPrefabs[0], hit.point, dropPoint.rotation);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Set Drop value
    /// </summary>
    /// <param name="val"></param>
    public void SetDrop( bool val)
    {
        doDrop = val;
    }

}
