using UnityEngine;

namespace usky.PrecomputeUtil
{
	public sealed class uSkyPrecomputeUtil 
	{

		const int TRANSMITTANCE_WIDTH	= 256;
		const int TRANSMITTANCE_HEIGHT	= 64;

		const int INSCATTER_WIDTH		= 256; 
		const int INSCATTER_HEIGHT		= 128;

		public static Texture2D Transmittance2D, Inscatter2D;

		// Create two Texture2D to cache the final precomputed texture data
		private static void PrecomputeTextureResource (int TotalInscatterTextureHeight) 
		{

			if (!Transmittance2D) {
				Transmittance2D = new Texture2D (TRANSMITTANCE_WIDTH, TRANSMITTANCE_HEIGHT, TextureFormat.RGBAHalf, false, true );
				Transmittance2D.wrapMode = TextureWrapMode.Clamp;
				Transmittance2D.anisoLevel = 0;
				Transmittance2D.name = "Transmittance2D";
				Transmittance2D.hideFlags = HideFlags.DontSave;
			}
			
			if (Inscatter2D && Inscatter2D.height != TotalInscatterTextureHeight)
			{
				Inscatter2D.Resize(INSCATTER_WIDTH, TotalInscatterTextureHeight);
				return ;
			}
			else
			if (!Inscatter2D) {
				Inscatter2D = new Texture2D (INSCATTER_WIDTH, TotalInscatterTextureHeight, TextureFormat.RGBAHalf, false, true );
				Inscatter2D.wrapMode = TextureWrapMode.Clamp;
				Inscatter2D.anisoLevel = 0;
				Inscatter2D.name = "inscatter2D";
				Inscatter2D.hideFlags = HideFlags.DontSave;
			}
		}
		 
		static void InitPrecomputeUniform( int sample, Vector4 lambda)
		{		
			Shader.SetGlobalInt	("RES_R", sample);
			Shader.SetGlobalVector ("betaR", lambda);
		}

		static void ApplyPrecomputeTexturesToMaterial(){
			Shader.SetGlobalTexture("_Transmittance", Transmittance2D);
			Shader.SetGlobalTexture("_Inscatter", Inscatter2D);
		}

		public static void StartPrecompute (Material pMaterial, PrecomputeSettings pSetting)
		{

			if (!pMaterial ) 
				return;
			
			int TotalInscatterTextureHeight = INSCATTER_HEIGHT * (int)pSetting.inscatterAltitudeSample;
			
			PrecomputeTextureResource (TotalInscatterTextureHeight);

			InitPrecomputeUniform((int)pSetting.inscatterAltitudeSample,
				Lambda (pSetting.wavelengths, pSetting.skyTint, pSetting.atmosphereThickness));

			// Transmittance Texture
			RenderTexture TransmittanceRT = RenderTexture.GetTemporary (TRANSMITTANCE_WIDTH, TRANSMITTANCE_HEIGHT, 0, RenderTextureFormat.ARGBHalf);
			TransmittanceRT.wrapMode = TextureWrapMode.Clamp;
			
			Graphics.Blit (null, TransmittanceRT, pMaterial, 0);
			
			RenderTexture.active = TransmittanceRT;
			
			Transmittance2D.ReadPixels (new Rect (0, 0, TRANSMITTANCE_WIDTH, TRANSMITTANCE_HEIGHT), 0, 0, false);
			Transmittance2D.Apply (false);
			
			// Inscatter Texture
			RenderTexture InscatterRT = RenderTexture.GetTemporary (INSCATTER_WIDTH, TotalInscatterTextureHeight, 0, RenderTextureFormat.ARGBHalf);
			InscatterRT.wrapMode = TextureWrapMode.Clamp;
			
			Graphics.Blit (Transmittance2D, InscatterRT, pMaterial, 1); 
			
			RenderTexture.active = InscatterRT;
			
			Inscatter2D.ReadPixels (new Rect (0, 0, INSCATTER_WIDTH, TotalInscatterTextureHeight), 0, 0, false);
			Inscatter2D.Apply (false);

			// clean up
			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary (TransmittanceRT);
			RenderTexture.ReleaseTemporary (InscatterRT);
		
			ApplyPrecomputeTexturesToMaterial ();

//			Debug.Log ("PrecomputeTextures updated! DepthSample = " + (int)pSetting.inscatterAltitudeSample );

		}

		static Vector4 Lambda ( Vector3 Wavelengths, Color SkyTint, float AtmosphereThickness){
			// Sky Tint shifts the value of Wavelengths
			Vector3 variableRangeWavelengths = 
			new Vector3 (Mathf.Lerp ( Wavelengths.x + 150f, Wavelengths.x - 150f, SkyTint.r ),
			             Mathf.Lerp ( Wavelengths.y + 150f, Wavelengths.y - 150f, SkyTint.g ),
			             Mathf.Lerp ( Wavelengths.z + 150f, Wavelengths.z - 150f, SkyTint.b ));

			variableRangeWavelengths.x = Mathf.Clamp (variableRangeWavelengths.x, 380f, 780f);
			variableRangeWavelengths.y = Mathf.Clamp (variableRangeWavelengths.y, 380f, 780f);
			variableRangeWavelengths.z = Mathf.Clamp (variableRangeWavelengths.z, 380f, 780f);

			// Evaluate Beta Rayleigh function is based on A.J.Preetham
				
			Vector3 WL = variableRangeWavelengths * 1e-9f;

			const float n = 1.0003f;		// the index of refraction of air
			const float N = 2.545e25f;		// molecular density at sea level
			const float pn = 0.035f;		// depolatization factor for standard air
			
			Vector3 waveLength4 = new Vector3 (Mathf.Pow (WL.x, 4), Mathf.Pow (WL.y, 4), Mathf.Pow (WL.z, 4));
			Vector3 delta = 3.0f * N * waveLength4 * (6.0f - 7.0f * pn);
			float ray = (8 * Mathf.Pow (Mathf.PI, 3) * Mathf.Pow (n * n - 1.0f, 2) * (6.0f + 3.0f * pn));
			Vector3 betaR = new Vector3 (ray / delta.x, ray / delta.y, ray / delta.z) ;

			// Atmosphere Thickness ( Rayleigh ) scale
			const float Km = 1000.0f;		// kilo meter unit
			betaR *= Km * Mathf.Max (1e-2f, AtmosphereThickness);

			// w channel solves the Rayleigh Offset artifact issue
			return new Vector4( betaR.x, betaR.y,betaR.z, Mathf.Max( Mathf.Pow( AtmosphereThickness, Mathf.PI), 1f));
		
		}

	}
}
