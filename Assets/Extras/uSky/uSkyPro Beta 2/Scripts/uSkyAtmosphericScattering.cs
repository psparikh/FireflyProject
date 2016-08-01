// This script is based on Unity "The Black Smith Atmospherics Scattering" demo project
// This script will create a MeshFilter and MeshRenderer Components that requires by LightEvent CommandBuffer for Occlusion effects
// So this will have an additional one more drawcall, actually nothing to render :P.

using UnityEngine;
using UnityEngine.Rendering;
using System;

namespace usky
{
	/// <summary>
	/// This script is responsible to render atmospheric scattering effects
	/// This script needs to be attached to a GameObject and requires uSkyPro component.
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu ("uSkyPro/uSky Atmospherics Scattering")]
//	[RequireComponent(typeof (uSkyPro))]
	public class uSkyAtmosphericScattering : MonoBehaviour {

		public static uSkyAtmosphericScattering instance { get; private set; } 

//		[Header("World Scattering")]
//		[Range (0.0f,10.0f)]
//		[SerializeField] 
//		private float fogMultiplier = 1.0f; // temp hide, not in use. WIP
//		public float FogMultiplier {
//			get { return fogMultiplier;}
//			set { fogMultiplier = value;}
//		}
		[SerializeField][HeaderLayout]
		private WorldInscatterSettings worldInscatter = new WorldInscatterSettings{
			enablePostEffect = true,
			scatteringIntensity = 1.0f,
			scatterExtinction = 0.0f,
			worldScatterScale = 4.0f,
			nearScatterPush = 300f,
		};

		[SerializeField][HeaderLayout]
		private ScatterOcclusionSettings scatterOcclusion = new ScatterOcclusionSettings
		{
			useOcclusion			= true,
			occlusionDarkness		= 0.0f,
			occlusionDownscale		= OcclusionDownscaleMode.x2,
			occlusionSamples		= OcclusionSamplesMode.x64,
		};

		uSkySun activeSun { get { return uSkySun.instance; }}
		Light activeLight;

		[HideInInspector] public Shader occlusionShader;

		bool			m_isAwake;

//		Camera			m_currentCamera;
		Material		m_occlusionMaterial;
		Texture2D		m_WhiteTex;

		CommandBuffer	m_occlusionCmdAfterShadows, m_occlusionCmdBeforeScreen;

		void CreateDummyWhiteTexture ()
		{
			if (m_WhiteTex != null)
				return;
			
			m_WhiteTex = new Texture2D (2, 2,TextureFormat.RGB24 ,false, true);
			m_WhiteTex.hideFlags = HideFlags.HideAndDontSave;
			for (int y = 0; y < 2; y++) {
				for (int x = 0; x < 2; x++) {
					m_WhiteTex.SetPixel (x, y, Color.white);
				}
			}
			m_WhiteTex.Apply (false);
		}

		void Awake() {
			if (!GetComponent<MeshFilter> ()) {
				MeshFilter mf = gameObject.AddComponent<MeshFilter> ();
				mf.sharedMesh = new Mesh ();
				mf.sharedMesh.bounds = new Bounds (Vector3.zero, Vector3.one * 2e9f); // 10000f);
				mf.sharedMesh.SetTriangles ((int[])null, 0);
				mf.hideFlags = HideFlags.HideInInspector;
			}

			if(!GetComponent<MeshRenderer>()) {
				MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
//				mr.useLightProbes = false; // this api removed in Unity 5.4
				mr.receiveShadows = false;
				mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
				mr.hideFlags = HideFlags.HideInInspector;
			}

			if(occlusionShader == null)
				occlusionShader = Shader.Find("Hidden/AtmosphericScattering_Occlusion");

			m_occlusionMaterial = new Material(occlusionShader);
			m_occlusionMaterial.hideFlags = HideFlags.HideAndDontSave;

			CreateDummyWhiteTexture ();

			m_isAwake = true;
		}
			
		void OnEnable() {
			if(!m_isAwake)
				return;

			UpdateAtmosphereUniform ();

			if(instance && instance != this)
				Debug.LogErrorFormat("Unexpected: uSkyAtmosphericScattering.instance already set (to: {0}). Still overriding with: {1}.", instance.name, name);
			
			instance = this;

			if (activeSun && activeLight == null)
				activeLight = activeSun.GetComponent<Light>();
			
			EnsureHookedLightSource(activeLight);
		}

		void EnsureHookedLightSource(Light light) {
			if(!light)
				return;
			
			if(m_occlusionCmdAfterShadows != null)
				m_occlusionCmdAfterShadows.Dispose();
			if(m_occlusionCmdBeforeScreen != null)
				m_occlusionCmdBeforeScreen.Dispose();

			m_occlusionCmdAfterShadows = new CommandBuffer();
			m_occlusionCmdAfterShadows.name = "Scatter Occlusion Pass 1";
			m_occlusionCmdAfterShadows.SetGlobalTexture("u_CascadedShadowMap", new RenderTargetIdentifier (BuiltinRenderTextureType.CurrentActive));

			m_occlusionCmdBeforeScreen = new CommandBuffer();
			m_occlusionCmdBeforeScreen.name = "Scatter Occlusion Pass 2";

			light.AddCommandBuffer(LightEvent.AfterShadowMap, m_occlusionCmdAfterShadows);
			light.AddCommandBuffer(LightEvent.BeforeScreenspaceMask, m_occlusionCmdBeforeScreen);
		}

		void OnDisable() {
			instance = null;
		}

		public void OnValidate() {
			if(!m_isAwake)
				return;

//			FogMultiplier = Mathf.Max (0f, FogMultiplier);
			ScatteringIntensity	= Mathf.Max (0f, ScatteringIntensity);
			ScatterExtinction	= Mathf.Clamp01 (ScatterExtinction);
			WorldScatterScale	= Mathf.Max ( 1f, WorldScatterScale);
			NearScatterPush		= Mathf.Max ( 0f, NearScatterPush);

		}

		void OnWillRenderObject() {
			if(!m_isAwake)
				Awake ();

			if(!activeSun) {
				// When there's no primary light, mie scattering and occlusion will be disabled, so there's
				// nothing for us to update.
				return;
			}

			UpdateAtmosphereUniform ();

			if (scatterOcclusion.useOcclusion) {
				Rect srcRect = new Rect (0f, 0f, Screen.width,Screen.height);
				var downscale = 1f / (float)(int)OcclusionDownscale;
				var occWidth = Mathf.RoundToInt (srcRect.width * downscale);
				var occHeight = Mathf.RoundToInt (srcRect.height * downscale);
				var occlusionId = Shader.PropertyToID ("u_OcclusionTexture");

				/*
				* If the current camera position is out of the shadow range, m_occlusionCmdBeforeScreen (CommandBuffer) will stop to execute.
				* In this case the Occlusion Texture will be null and get incorrect output, so we assign a dummy white texture (2x2 pixels) here.
				* Whenever m_occlusionCmdBeforeScreen get executed, "occlusionId" will be assigned automatically with Occlusion Texture again.
				*/
				Shader.SetGlobalTexture (occlusionId, m_WhiteTex );

				if (m_occlusionCmdBeforeScreen != null) { // avoid "NullReferenceException" error warning spams in editor
					m_occlusionCmdBeforeScreen.Clear ();
					m_occlusionCmdBeforeScreen.GetTemporaryRT (occlusionId, occWidth, occHeight, 0, FilterMode.Bilinear, RenderTextureFormat.R8, RenderTextureReadWrite.sRGB);
					m_occlusionCmdBeforeScreen.Blit (
						(Texture)null, 
						occlusionId,
						m_occlusionMaterial,
						(int)OcclusionSamples
					);
					m_occlusionCmdBeforeScreen.SetGlobalTexture (occlusionId, occlusionId);
					m_occlusionCmdBeforeScreen.ReleaseTemporaryRT(occlusionId);
				}
			}
		}

		void UpdateAtmosphereUniform (){	
//			Shader.SetGlobalFloat ("_AtmosphereFogMultiplier", FogMultiplier);
			Shader.SetGlobalFloat ("_ScatteringIntensity",	ScatteringIntensity);
			Shader.SetGlobalFloat ("_ScatterExtinction",	ScatterExtinction);
			Shader.SetGlobalFloat ("_AtmosphereWorldScale",	WorldScatterScale);
			Shader.SetGlobalFloat ("_NearScatterPush",		NearScatterPush );

			// (AtmosphereThickness to dim the night horizon fog)
	//		Shader.SetGlobalVector ("_NightHorizonColorDeferred", m_NightHorizonColor * 0.5f * NightFade / Mathf.Max ( Mathf.Pow (PrecomputeParams.AtmosphereThickness, 0.25f), 1));
			Shader.SetGlobalVector ("_OcclusionDarkness",  new Vector3 (OcclusionDarkness, 0f, scatterOcclusion.useOcclusion? 1f : 0f));
		}

// Properties accessor --------------------
		public bool EnablePostEffect {
			get { return worldInscatter.enablePostEffect;}
			set { worldInscatter.enablePostEffect = value;}
		}

		public float ScatteringIntensity {
			get { return worldInscatter.scatteringIntensity;}
			set { worldInscatter.scatteringIntensity = value;}
		}

		public float ScatterExtinction {
			get { return worldInscatter.scatterExtinction;}
			set { worldInscatter.scatterExtinction = value;}
		}

		public float WorldScatterScale {
			get { return worldInscatter.worldScatterScale;}
			set { worldInscatter.worldScatterScale = value;}
		}

		public float NearScatterPush {
			get { return worldInscatter.nearScatterPush;}
			set { worldInscatter.nearScatterPush = value;}
		}

	// Occlusion ---------------------------------

		// use the UseScatterOcclusion instead
//		private bool UseOcclusion {
//			get { return scatterOcclusion.useOcclusion;}
//			set { scatterOcclusion.useOcclusion = value;}
//		}

		// access by other script or UI at runtime
		public void UseScatterOcclusion (bool NewBoolean){
			scatterOcclusion.useOcclusion = NewBoolean;
		}

		public float OcclusionDarkness {
			get { return scatterOcclusion.occlusionDarkness;}
			set { scatterOcclusion.occlusionDarkness = value;}
		}

		public OcclusionDownscaleMode OcclusionDownscale {
			get { return scatterOcclusion.occlusionDownscale;}
			set { scatterOcclusion.occlusionDownscale = value;}
		}

		public OcclusionSamplesMode OcclusionSamples {
			get { return scatterOcclusion.occlusionSamples;}
			set { scatterOcclusion.occlusionSamples = value;}
		}

	}
}