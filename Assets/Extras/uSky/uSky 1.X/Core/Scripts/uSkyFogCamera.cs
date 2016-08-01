using UnityEngine;

namespace uSky
{
	[ExecuteInEditMode]
	[RequireComponent (typeof(Camera))]
	[AddComponentMenu ("uSky/uSky Fog Camera (Image Effects)")]
	public class uSkyFogCamera : MonoBehaviour
	{
		public Material fogMaterial;
		Camera cam;

		protected bool CheckSupport ()
		{
			// support ImageEffects?
			if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures) {
//				enabled = false;
				return false;
			}
			// support depth?
			if (!SystemInfo.SupportsRenderTextureFormat (RenderTextureFormat.Depth)) {
//				enabled = false;
				return false;
			}

			cam.depthTextureMode |= DepthTextureMode.Depth;
			
			return true;
		}

		void OnEnable (){
			cam = GetComponent<Camera> ();
		}
		
		[ImageEffectOpaque]
		void OnRenderImage (RenderTexture source, RenderTexture destination)
		{
			if (CheckSupport () == false) 
			{
				Graphics.Blit (source, destination);
				return;
			}
			
			Transform camtr = cam.transform;
			float camNear = cam.nearClipPlane;
			float camFar = cam.farClipPlane;
			float camFov = cam.fieldOfView;
			float camAspect = cam.aspect;

	        Matrix4x4 frustumCorners = Matrix4x4.identity;

			float fovWHalf = camFov * 0.5f;
//
			Vector3 toRight = camtr.right * camNear * Mathf.Tan (fovWHalf * Mathf.Deg2Rad) * camAspect;
			Vector3 toTop = camtr.up * camNear * Mathf.Tan (fovWHalf * Mathf.Deg2Rad);

			Vector3 topLeft = (camtr.forward * camNear - toRight + toTop);
			float camScale = topLeft.magnitude * camFar/camNear;

	        topLeft.Normalize();
			topLeft *= camScale;

			Vector3 topRight = (camtr.forward * camNear + toRight + toTop);
	        topRight.Normalize();
			topRight *= camScale;

			Vector3 bottomRight = (camtr.forward * camNear + toRight - toTop);
	        bottomRight.Normalize();
			bottomRight *= camScale;

			Vector3 bottomLeft = (camtr.forward * camNear - toRight - toTop);
	        bottomLeft.Normalize();
			bottomLeft *= camScale;

			frustumCorners.SetRow (0, topLeft);
	        frustumCorners.SetRow (1, topRight);
	        frustumCorners.SetRow (2, bottomRight);
	        frustumCorners.SetRow (3, bottomLeft);

	        fogMaterial.SetMatrix ("_FrustumCornersWS", frustumCorners);
			fogMaterial.SetVector ("_CameraWS", camtr.position);

			CustomGraphicsBlit (source, destination, fogMaterial, 0);
	    }

	    static void CustomGraphicsBlit (RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr)
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