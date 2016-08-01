#if !( UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 )
#define UNITY_540_OR_HIGHER
#endif

using UnityEngine;

namespace usky
{
	/// <summary>
	/// This is the main camera image effect for atmospheric scattering.
	/// This script needs to be attached to a Main Camera GameObject.
	/// </summary>

/*	New feature in Unity 5.4
	This will copy the Image effect from the main camera onto the Scene View camera. 
	This can be enabled / disabled in the Scene View effects menu. */
#if UNITY_540_OR_HIGHER
	[ImageEffectAllowedInSceneView]
#endif

	[ExecuteInEditMode][DisallowMultipleComponent]
	[AddComponentMenu ("uSkyPro/uSky Atmospheric Camera")]
	public class uSkyAtmosphericCamera : MonoBehaviour
	{
		[HideInInspector] public Shader m_AtmosphericCameraShader;
		Material m_AtmosphericCameraMaterial;
		Camera cam;

		protected bool CheckSupport ()
		{
			// support ImageEffects?
			if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures) {
				return false;
			}
			// support depth?
			if (!SystemInfo.SupportsRenderTextureFormat (RenderTextureFormat.Depth)) {
				return false;
			}

			cam.depthTextureMode |= DepthTextureMode.Depth;
			
			return true;
		}

		Material CheckMaterial (){
			if (m_AtmosphericCameraMaterial && m_AtmosphericCameraMaterial.shader == m_AtmosphericCameraShader)
				return m_AtmosphericCameraMaterial;

			if (m_AtmosphericCameraShader == null)
				m_AtmosphericCameraShader = Shader.Find("Hidden/uSkyPro AtmosphericScattering Deferred");
			
			m_AtmosphericCameraMaterial = new Material (m_AtmosphericCameraShader);
			m_AtmosphericCameraMaterial.hideFlags = HideFlags.DontSave;
			m_AtmosphericCameraMaterial.SetInt("_uSkySkyboxOcean", 0 );
			return m_AtmosphericCameraMaterial;
		}

		void OnEnable (){
			m_AtmosphericCameraMaterial = CheckMaterial ();
			cam = GetComponent<Camera> ();
		}

		[ImageEffectOpaque]
		void OnRenderImage(RenderTexture source, RenderTexture destination) 
		{	
			bool shouldRender =  CheckSupport() && ((cam && uSkyPro.instance )
			          &&  (uSkyAtmosphericScattering.instance && uSkyAtmosphericScattering.instance.EnablePostEffect));

			if (!shouldRender){ 
				Graphics.Blit(source, destination);
				return;
			}

			CustomGraphicsBlit(source, destination, m_AtmosphericCameraMaterial, 0);
		}
		
		static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr) 
		{
			RenderTexture.active = dest;
			       
			fxMaterial.SetTexture ("_MainTex", source);	        
		        	        
			GL.PushMatrix ();
			GL.LoadOrtho ();
		    	
			fxMaterial.SetPass (passNr);	
			
		    GL.Begin (GL.QUADS);

			GL.MultiTexCoord2 (0, 0.0f, 0.0f); 
			GL.Vertex3 (0.0f, 0.0f, 3.0f); // BL
			
			GL.MultiTexCoord2 (0, 1.0f, 0.0f); 
			GL.Vertex3 (1.0f, 0.0f, 2.0f); // BR
			
			GL.MultiTexCoord2 (0, 1.0f, 1.0f); 
			GL.Vertex3 (1.0f, 1.0f, 1.0f); // TR
			
			GL.MultiTexCoord2 (0, 0.0f, 1.0f); 
			GL.Vertex3 (0.0f, 1.0f, 0.0f); // TL
			
			GL.End ();
		    GL.PopMatrix ();
			
		}	
	}
}