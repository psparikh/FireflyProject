using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Breakable: MonoBehaviour
{
    private List<GameObject> triangles;
    private GameObject[] trianglesAll;

    public Vector3 offset;

    public float radius;
    public float power;

    public float upwardsModifier = 3.0F;

    private bool isSplit;

#if UNITY_EDITOR
    public bool breakCheck;
    public bool dissolve;
#endif

    private Mesh currentMesh;
    private Material[] currentMaterial;


    void Awake()
    {
        isSplit = false;
        triangles = new List<GameObject>();
        RetrieveMesh();
        RetrieveMaterials();
    }


    void Update()
    {

#if UNITY_EDITOR
        if( breakCheck)
        {
            StartCoroutine( Break() );
            breakCheck = !breakCheck;
        }
        
        if (dissolve)
        {
            StartCoroutine(Dissolve());
        }
        
    }
#endif


    public void OnCollisionEnter(Collision col)
    {
        if (col.collider.tag == "bullet")
        {

            Vector3 contactPoint = col.contacts[0].point;
            StartCoroutine( Break(contactPoint) );
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position + offset, radius);
    }

    /// <summary>
    /// Break Object at Position
    /// </summary>
    /// <param name="hitPos"></param>
    public IEnumerator Break( Vector3 hitPos )
    {
        yield return StartCoroutine(Fragment());
        Shatter(hitPos + offset);

    }

    public IEnumerator Break()
    {
        yield return StartCoroutine(Fragment());
        Shatter(transform.position + offset);
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
    }

    /// <summary>
    /// Fragments by splitting mesh and hiding original renderer
    /// </summary>
    /// <returns></returns>
    private IEnumerator Fragment()
    {
        if (!isSplit)
        {
            yield return StartCoroutine(SplitMesh());
            HideOriginal();
            isSplit = true;
        }
    }


    /// <summary>
    /// Split the mesh into its triangles
    /// </summary>
    /// <returns></returns>
    private IEnumerator SplitMesh()
    {
        //Deactivate parent collider
        //if (GetComponent<Collider>())
        //{
        //    GetComponent<Collider>().enabled = false;
        //}

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

                triangles.Add(tri);

                tri.transform.SetParent(transform);

            }
        }

        trianglesAll = triangles.ToArray();
        yield return null;

    }


    private void HideOriginal()
    {
        GetComponent<Renderer>().enabled = false;
    }

    /// <summary>
    /// Destroys original object
    /// </summary>
    private void DestroyOriginal()
    {
        triangles.Clear();
        Destroy(gameObject, 6.0f);
    }

    /// <summary>
    /// Shatter Object
    /// </summary>
    private void Shatter( Vector3 hitPos )
    {

        Vector3 explosionPos = hitPos;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius, ~(LayerMask.NameToLayer("triangle")));

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.transform.SetParent(null);

                rb.useGravity = true;
                rb.isKinematic = false;

                Vector3 pos = new Vector3(explosionPos.x + Random.Range(-0.5f, 0.5f), explosionPos.y + Random.Range(-0.5f, 0.5f), explosionPos.z + Random.Range(-0.5f, 0.5f));

                rb.AddExplosionForce(power, pos, radius, upwardsModifier);
                Destroy(rb.gameObject, Random.Range(3.0f, 5.0f));

                triangles.Remove(rb.gameObject);
            }

        }

    }

    private void ShatterAll()
    {
        foreach (GameObject go in trianglesAll)
        {
            if (go != null)
            {
                Rigidbody rb = go.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.transform.SetParent(null);

                    rb.useGravity = true;
                    rb.isKinematic = false;

                    Vector3 pos = new Vector3(transform.position.x + Random.Range(-0.5f, 0.5f),
                                                transform.position.y + Random.Range(-0.5f, 0.5f),
                                                transform.position.z + Random.Range(-0.5f, 0.5f));

                    rb.AddExplosionForce(power, pos, radius);
                    Destroy(rb.gameObject, Random.Range(3.0f, 5.0f));

                    triangles.Remove(rb.gameObject);
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
