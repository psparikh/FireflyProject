// Note: The Hemisphere Mesh must check the "Read/Write Enable" from the import setting.

using UnityEngine;

namespace uSky
{
	[ExecuteInEditMode]
	[AddComponentMenu("uSky/Distance Cloud")]
	[RequireComponent (typeof (uSkyManager),typeof (uSkyLight))]
	public class DistanceCloud : MonoBehaviour {

		public Material CloudMaterial;
//		[Tooltip ("Override the brightness at night time.")]
		public float NightBrightness = 0.25f;
		public int cloudLayer = 12;
		public bool RenderCloudsDome = true;

		Mesh skyDome;
		uSkyManager uSM;

		private uSkyLight m_uSL;
		protected uSkyLight uSL {
			get{
				if (m_uSL == null) {
					m_uSL = this.gameObject.GetComponent<uSkyLight>();
					if (m_uSL == null)
						Debug.Log("Can't not find uSkyLight Component, Please apply DistanceCloud in uSkyManager gameobject");
				}
				return m_uSL;
			}
		}

		protected void InitSkyDomeMesh (){
			Mesh Hemisphere = Resources.Load<Mesh> ("Hemisphere_Mesh") as Mesh;
			if (Hemisphere == null) {
				Debug.Log ("Can't find Hemisphere_Mesh.fbx file.");
			} else {
				skyDome = (Mesh) Instantiate( Hemisphere , Vector3.zero , Quaternion.identity);
				skyDome.bounds = new Bounds (Vector3.zero, Vector3.one * 2e9f);
				skyDome.hideFlags = HideFlags.DontSave;
			}
		}

		void OnEnable (){
			if (skyDome == null && RenderCloudsDome)
				InitSkyDomeMesh ();
		}

		void OnDestroy() {
			if (skyDome) 
				DestroyImmediate(skyDome);
		}

		void Start () {
			uSM = uSkyManager.instance;
			if (uSM != null)
				if (!uSM.SkyUpdate && uSL != null)
					UpdateCloudMaterial ();
		}
		
		// Update is called once per frame
		void Update (){
			if (uSM != null)
				if (uSM.SkyUpdate && uSL != null)
					UpdateCloudMaterial ();

			if (skyDome && CloudMaterial && RenderCloudsDome)
				Graphics.DrawMesh (skyDome, Vector3.zero, Quaternion.identity, CloudMaterial, cloudLayer );
		}

		void UpdateCloudMaterial () {
			 
			float	Brightness = Mathf.Max ( Mathf.Pow ( NightBrightness, uSM.LinearSpace ? 1.5f : 1f) , uSM.DayTime); 
					Brightness *= Mathf.Sqrt( uSM.Exposure); // sync with sky Exposure?

				Shader.SetGlobalVector ("ShadeColorFromSun",uSM.LinearSpace ? uSL.CurrentLightColor.linear * Brightness : uSL.CurrentLightColor * Brightness);
				Shader.SetGlobalVector("ShadeColorFromSky",uSM.LinearSpace ? uSL.CurrentSkyColor.linear * Brightness : uSL.CurrentSkyColor * Brightness);
		}

		public void OnValidate() {
			
			if (enabled && base.isActiveAndEnabled) {
				OnEnable ();
			}
		}

	}
}