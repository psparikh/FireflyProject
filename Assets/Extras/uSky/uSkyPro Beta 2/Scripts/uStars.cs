using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace usky
{
	public class uStars 
	{
		private float starSizeScale = 1.0f; // if modify this value that will update after click PLAY button in Editor

		private List<CombineInstance> starQuad = new List<CombineInstance>();
		
		private struct Star
		{
			public Vector3 position;
			public Vector4 color;
		}

		public void InitializeStarfield( ref Mesh mesh)
		{

			float starDistance = 990f ; // range within the default camera far clipping plane 1000
			float starSize = starDistance / 100f * starSizeScale;

			// Load star positions and colors from file with 9110 predefined stars.
			TextAsset data =  Resources.Load<TextAsset>("StarsData");
			if (data == null){
				Debug.LogError("Can't find or read StarsData.bytes file.");
				return;
			}

			const int numberOfStars = 9110;
			var stars = new Star[numberOfStars];

			using (BinaryReader reader = new BinaryReader(new MemoryStream(data.bytes)))
			{
				for (int i = 0; i < numberOfStars; i++)
				{
					stars[i].position.x = reader.ReadSingle();
					stars[i].position.z = reader.ReadSingle();
					stars[i].position.y = reader.ReadSingle(); // Z-up to Y-up

					stars[i].position = Vector3.Scale (stars[i].position,new Vector3(-1f,1f,-1f));

					stars[i].color.x = reader.ReadSingle();
					stars[i].color.y = reader.ReadSingle();
					stars[i].color.z = reader.ReadSingle();

					// Using Vector3.magnitude term to sort the brightness for magnitude
					stars[i].color.w = new Vector3 ( stars[i].color.x , stars[i].color.y , stars[i].color.z).magnitude;

					// fix an over bright star (Sirius)?
					if (stars [i].color.w > 5.7) {
						stars [i].color = Vector4.Normalize (stars [i].color)* 0.5f;
					}
					/*
 						Note: To improve performance, we sort stars by brightness and remove less important stars.
 						
						Threshold:
						6.225e-2f	 // 1024 predefined stars.
						3.613e-2f	 // 2047 predefined stars.
						2.0344e-2f	 // 4096 predefined stars.
					*/
					#if  UNITY_IOS || UNITY_ANDROID || UNITY_BLACKBERRY
						float threshold = 6.225e-2f;	// 1024 predefined stars.
					#elif UNITY_WEBGL
						float threshold = 3.613e-2f;
					#else
						float threshold = 2.044e-2f;	// 4096 predefined stars
					#endif

					if(stars[i].color.w < threshold) continue;

					CombineInstance ci = new CombineInstance();
					ci.mesh = createQuad(starSize);

					ci.transform = BillboardMatrix(stars[i].position * starDistance);

					Color[] colors = {stars[i].color,stars[i].color,stars[i].color,stars[i].color};
					ci.mesh.colors = colors;

					starQuad.Add(ci);
				}
			}
			// -------------------------------------------
			// Combine Quad Meshes
			mesh.name = "StarFieldMesh";
			mesh.CombineMeshes(starQuad.ToArray());
			mesh.Optimize();
			// over size mesh bounds to avoid camera frustum culling for Vertex transformation in shader 
			mesh.bounds = new Bounds ( Vector3.zero, Vector3.one * 2e9f); // less than 2,147,483,648
			mesh.hideFlags = HideFlags.HideAndDontSave;
		}

		private Mesh createQuad (float size){

			Vector3[] Vertices = 
			{
				// 4 vertexs for 2 triangles 
				new Vector3( 1, 1, 0) * size,
				new Vector3(-1, 1, 0) * size,
				new Vector3( 1,-1, 0) * size,
				new Vector3(-1,-1, 0) * size
			};

			Vector2[] uv = 
			{ 
				// 2 triangles uv
				new Vector2(0, 1), 
				new Vector2(1, 1),
				new Vector2(0, 0),
				new Vector2(1, 0)
			};

			int[] triangles = new int[6]
			{
				// 2 triangles
				0, 2, 1,
				2, 3, 1
			};

			Mesh m = new Mesh();
			
			m.vertices = Vertices;
			m.uv = uv;
			m.triangles = triangles;
			m.RecalculateNormals();
			m.name = "StarSprite"; // debug
			m.hideFlags = HideFlags.HideAndDontSave;
			return m;
		}

		// Billboard will facing the center origin of the GameObject pivot 
		private Matrix4x4 BillboardMatrix (Vector3 particlePosition)
		{
			Vector3 direction = particlePosition - Vector3.zero;
			direction.Normalize();
			
			Vector3 particleRight = Vector3.Cross(direction, Vector3.up);
			particleRight.Normalize();
			
			Vector3 particleUp = Vector3.Cross(particleRight, direction);
			
			Matrix4x4 matrix = new Matrix4x4();

			matrix.SetColumn(0, particleRight);		// right
			matrix.SetColumn(1, particleUp);		// up
			matrix.SetColumn(2, direction);			// forward
			matrix.SetColumn(3, particlePosition);	// position

			return matrix;
		}
	}
}