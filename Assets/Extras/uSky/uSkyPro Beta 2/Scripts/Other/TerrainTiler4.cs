using UnityEngine;
using System.Collections;

namespace usky
{

	[AddComponentMenu("uSkyPro/Other/Terrain Tiler 4")]

	public class TerrainTiler4 : MonoBehaviour {

		public Terrain[] terrains = new Terrain[4];

		/*  Expects an array with max 4 entries
		*   Layed out like:
		*	Top View
		*	(Z - Axis)
		*	0	1
		*	2	3	(X - Axis)
		*/
		
		// Use this for initialization
		void Awake () {
			if (!terrains[0] || !terrains[1] || !terrains[2] || !terrains[3]) 
				return;

			if (terrains.Length != 4)
				throw new System.Exception ("Incorrect number of terrains");

			// Terrain.activeTerrain.SetNeighbors ( left, top, right, bottom);
			terrains[0].SetNeighbors( null, null , terrains[1], terrains[2] );
			terrains[1].SetNeighbors( terrains[0], null, null, terrains[3] );
			terrains[2].SetNeighbors( null, terrains[0] , terrains[3] , null );
			terrains[3].SetNeighbors( terrains[2] , terrains[1], null, null );

	//		Debug.Log ("Tiled done!");
		}

	}
}
