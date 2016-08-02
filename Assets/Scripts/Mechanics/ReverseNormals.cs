using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class ReverseNormals : MonoBehaviour
{

    // n.b. NEVER modify sharedMesh; that will edit the Mesh asset itself
    public Mesh sharedMesh;
    public MeshFilter mf;
    public MeshCollider mc;

    private bool initialized;

    void Start()
    {

        if (!initialized)
        {

            Mesh mesh = new Mesh();
            mesh.vertices = sharedMesh.vertices;
            mesh.normals = sharedMesh.normals;
            mesh.triangles = sharedMesh.triangles;
            mesh.uv = sharedMesh.uv;

            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; ++i)
            {
                normals[i] = -normals[i];
            }
            mesh.normals = normals;

            for (int subMesh = 0; subMesh < mesh.subMeshCount; ++subMesh)
            {

                int[] triangles = mesh.GetTriangles(subMesh);

                for (int i = 0; i < triangles.Length; i += 3)
                {

                    int tri = triangles[i];
                    triangles[i] = triangles[i + 1];
                    triangles[i + 1] = tri;

                }
                mesh.SetTriangles(triangles, subMesh);
            }

            if (mf) { mf.mesh = mesh; }

            if (mc) { mc.sharedMesh = mesh; }

            initialized = true;

        }

    }

}