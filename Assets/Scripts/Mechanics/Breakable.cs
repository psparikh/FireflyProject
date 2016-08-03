using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Breakable: MonoBehaviour
{
    private List<GameObject> activeTriangles;
    private GameObject[] triangles;

    //Determine dissolve blast parameters
    public Vector3 offset = Vector3.zero;
    public float radius = 1.0f;
    public float power = 100.0f;
    public float upwardsModifier = 3.0F;

    //Determine shatter section size
    public float shatterPercentage;

    public bool fragmentOnStart = false;
    private bool isFragmented;

#if UNITY_EDITOR
    public bool dissolve;
#endif

    private Mesh currentMesh;
    private Material[] currentMaterial;


    void Awake()
    {
        isFragmented = false;
        activeTriangles = new List<GameObject>();
        RetrieveMesh();
        RetrieveMaterials();
    }

    void Start()
    {
        if(fragmentOnStart && !isFragmented)
        {
            StartCoroutine(Fragment());
            isFragmented = true;
        }
    }


    void Update()
    {
#if UNITY_EDITOR        
        if (dissolve)
            StartCoroutine(Dissolve());
#endif
    }


    public void OnTriggerEnter(Collider col)
    {
        if (col.tag == "bullet")
        {
            StartCoroutine(Dissolve());
        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position + offset, radius);
    }


    /// <summary>
    /// Dissolves object completely
    /// </summary>
    /// <returns></returns>
    public IEnumerator Dissolve()
    {
        yield return StartCoroutine(Fragment());
        ShatterAll();
        DestroyOriginal();
        yield return null;

    }


    /// <summary>
    /// Fragments by splitting mesh and hiding original renderer
    /// </summary>
    /// <returns></returns>
    private IEnumerator Fragment()
    {
        if (!isFragmented)
        {
            yield return StartCoroutine(SplitMesh());
            GetComponent<Renderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
            isFragmented = true;
        }
        yield return null;
    }


    /// <summary>
    /// Split the mesh into its triangles
    /// </summary>
    /// <returns></returns>
    private IEnumerator SplitMesh()
    {
        Vector3[] verts = currentMesh.vertices;
        Vector3[] normals = currentMesh.normals;
        Vector2[] uvs = currentMesh.uv;

        for (int submesh = 0; submesh < currentMesh.subMeshCount; submesh++)
        {
            int[] indices = currentMesh.GetTriangles(submesh);
            for (int i = 0; i < indices.Length; i += 3)
            {
                Vector3[] newVerts = new Vector3[3];
                Vector3[] newNormals = new Vector3[3];
                Vector2[] newUvs = new Vector2[3];
                for (int n = 0; n < 3; n++)
                {
                    int index = indices[i + n];
                    newVerts[n] = verts[index];
                    newUvs[n] = uvs[index];
                    newNormals[n] = normals[index];
                }
                Mesh mesh = new Mesh();
                mesh.vertices = newVerts;
                mesh.normals = newNormals;
                mesh.uv = newUvs;
                mesh.triangles = new int[] { 0, 1, 2, 2, 1, 0 };

                GameObject tri = new GameObject("triangle " + (i / 3));
                tri.layer = LayerMask.NameToLayer("triangle");

                tri.transform.localScale = transform.localScale;
                tri.transform.position = transform.position;
                tri.transform.rotation = transform.rotation;
                tri.AddComponent<MeshRenderer>().material = currentMaterial[submesh];
                tri.AddComponent<MeshFilter>().mesh = mesh;

                tri.AddComponent<BoxCollider>();
                tri.AddComponent<Rigidbody>();

                tri.GetComponent<Rigidbody>().useGravity = false;
                tri.GetComponent<Rigidbody>().isKinematic = true;

                tri.AddComponent<Shatter>();
                tri.GetComponent<Shatter>().multiplier = shatterPercentage;

                activeTriangles.Add(tri);

                tri.transform.SetParent(transform);
            }
        }

        triangles = activeTriangles.ToArray();
        yield return null;

    }


    /// <summary>
    /// Hides original mesh so its only used as placeholder
    /// </summary>
    private void HideOriginal()
    {
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }


    /// <summary>
    /// Destroys original object
    /// </summary>
    private void DestroyOriginal()
    {
        activeTriangles.Clear();
        Destroy(gameObject, 6.0f);
    }


    /// <summary>
    /// Shatters whole object
    /// </summary>
    private void ShatterAll()
    {
        foreach (GameObject go in triangles)
        {
            if (go != null)
            {
                Rigidbody rb = go.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.transform.SetParent(null);

                    rb.useGravity = true;
                    rb.isKinematic = false;

                    Vector3 sourcePos = transform.position + offset;

                    Vector3 pos = new Vector3(sourcePos.x + Random.Range(-0.5f, 0.5f),
                                                sourcePos.y + Random.Range(-0.5f, 0.5f),
                                                sourcePos.z + Random.Range(-0.5f, 0.5f));

                    rb.AddExplosionForce(power, pos, radius);
                    Destroy(rb.gameObject, Random.Range(3.0f, 5.0f));

                    activeTriangles.Remove(rb.gameObject);
                }
            }
        }

    }


    /// <summary>
    /// Sets Mesh
    /// </summary>
    private void RetrieveMesh()
    {
        Mesh M = new Mesh();
        if (GetComponent<MeshFilter>())
        {
            M = GetComponent<MeshFilter>().mesh;
        }
        else if (GetComponent<SkinnedMeshRenderer>())
        {
            M = GetComponent<SkinnedMeshRenderer>().sharedMesh;
        }
        currentMesh = M;
    }


    /// <summary>
    /// Sets Materials
    /// </summary>
    private void RetrieveMaterials()
    {
        Material[] materials = new Material[0];
        if (GetComponent<MeshRenderer>())
        {
            materials = GetComponent<MeshRenderer>().materials;
        }
        else if (GetComponent<SkinnedMeshRenderer>())
        {
            materials = GetComponent<SkinnedMeshRenderer>().materials;
        }
        currentMaterial = materials;
    }


}
