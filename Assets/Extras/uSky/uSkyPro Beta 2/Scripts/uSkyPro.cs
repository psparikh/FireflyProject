/* ************
 * Known Issue
 * ************
 * [Editor] Occasionally the stars will become black in the Scene view window. Stars material requires the scene alpha to work.  
 * This black stars issue occurs only when the scene view meet all of the following settings :
 * - Scene view lighting is ON. (little sun icon from the Scene view window toolbar)
 * - Main Camera uses deferred mode.
 * - Main Camera uses HDR.
 * 
 * However the Stars are just working completely fine in Game view!
*/

using UnityEngine;
using usky;
using usky.PrecomputeUtil;
using usky.Internal;

/// <summary>
/// uSkyPro Manager is the main component of the uSky skybox abd atmospheric system.
/// This script needs to be attached to a GameObject.
/// It will work as standalone component with uSkySun and uSkyMoon.
/// </summary>

[HelpURL("http://forum.unity3d.com/threads/uskypro-precomputed-atmospheric-scattering-2-0-beta.268288/")]
[ExecuteInEditMode][DisallowMultipleComponent]
[AddComponentMenu("uSkyPro/uSkyPro Manager")]
public class uSkyPro : MonoBehaviour 
{
	public static uSkyPro instance { get; private set; }

	[Space()]
	[Range(0.0f, 10.0f)][Tooltip ("This value sets the brightness of the sky.")]
	[SerializeField] private float exposure = 1.0f;

	[Range(0.0f, 20.0f)][Tooltip ("Mie scattering is caused by aerosols in the lower atmosphere (up to 1.2 km).\nIt is for haze and halos around the sun on foggy days.")]
	[SerializeField] private float mieScattering = 1.0f;

	[Range (0f,0.9995f)][Tooltip ("The anisotropy factor controls the sun's appearance in the sky.\nThe closer this value gets to 1.0, the sharper and smaller the sun spot will be.\nHigher values cause more fuzzy and bigger sun spots.")]
	[SerializeField] private float sunAnisotropyFactor = 0.76f;

	[Range (1e-3f,10.0f)][Tooltip ("Size of the sun spot in the sky")]
	[SerializeField] private float sunSize = 1.0f;

	[SerializeField][HeaderLayout]
	private NightSkySettings nightSky = new NightSkySettings 
	{
		nightMode = NightModes.Rotation,
		nightZenithColor = new Color32 (051, 074, 102, 255),
		nightHorizonColor = new Color32(072, 100, 128, 128),
		starIntensity = 1.0f,
		outerSpaceIntensity = 0.25f,
		moonInnerCorona = new Color32(225, 225, 225, 128),
		moonOuterCorona = new Color32(65,88,128,128),
		moonSize = 1.0f
	};

	[SerializeField][HeaderLayout]
	private OtherSettings other = new OtherSettings 
	{
		groundOffset = 0.0f,
		altitudeScale = 1.0f,
		disableSkyboxOcean = false,
		HDRMode = false
	};
			
	// Note: Updating precompute data in runtime may have performance impact on low-end machine. 
	// Only use it in runtime if absolutly necessary.
	[SerializeField][HeaderLayout]
	private PrecomputeSettings precomputeParams = new PrecomputeSettings 
	{
		atmosphereThickness = 1.0f,
		wavelengths = new Vector3(680f, 550f, 440f),
		skyTint = new Color32(128, 128, 128, 255),
		inscatterAltitudeSample = DepthSample.X1
	};
			
	[Space()]
	public Material SkyboxMaterial;
	private bool AutoApplySkybox = true;

	[HideInInspector][SerializeField] 
	private Shader m_PrecomputeShader, m_uStarShader;
	Material m_PrecomputeMaterial, m_uStarsMaterial;

	uStars Star = new uStars ();
	private Mesh m_StarsMesh;

	private bool m_isAwake;

	uSkySun	m_Sun	{ get { return uSkySun.instance; } }
	uSkyMoon m_Moon	{ get { return uSkyMoon.instance; } }

	void CheckMaterialResource()
	{
		// Initialize shaders and materials
		if (m_PrecomputeShader == null)
			m_PrecomputeShader = Shader.Find ("Hidden/uSkyPro/Precompute");
		m_PrecomputeMaterial = new Material(m_PrecomputeShader);
		m_PrecomputeMaterial.hideFlags = HideFlags.HideAndDontSave;

		if (m_uStarShader == null) 
			m_uStarShader = Shader.Find ("Hidden/uSkyPro/uStars");
		m_uStarsMaterial = new Material (m_uStarShader);
		m_uStarsMaterial.hideFlags = HideFlags.HideAndDontSave;

	}

	void SetEnumsAndToggles()
	{
		uSkyInternal.SetNightSkyMode ((int)NightMode);
//		uSkyUtil.SetSkyboxOcean (DisableSkyboxOcean);
		uSkyInternal.SetHDRMode (HDRMode);
	}

	void Awake ()
	{
		if (!uSkySun.instance){
			GameObject sunLight = GameObject.Find ("Directional Light"); // default new scene
			if (sunLight == null)
				sunLight = GameObject.Find ("Directional light"); // new directional light
			if (sunLight != null) 
				sunLight.AddComponent <uSkySun>();
			else
				Debug.LogWarning ("Missing Sun! Please apply the <b>uSkySun</b> component to a Directional Light" );
		}

		CheckMaterialResource ();

		// Check if graphics api supports for shader model 3.0 or higher
		// It is automatically fallback for shader models 2.0 skybox which supports DepthSample X1 only
		if (SystemInfo.graphicsShaderLevel < 30)
			InscatterAltitudeSample = DepthSample.X1;

		m_isAwake = true;
	}

	void OnEnable ()
	{
		if (!m_isAwake)
			Awake ();
		
		SetEnumsAndToggles ();
		UpdateKeywords();
		UpdateMaterialUniform ();

		// Initialize all the parameters before any event added
		uSkyInternal.InitAtmosphereParameters (this);

//		UpdateAmbientSH (); // TODO: Update unity builtin SH, may not need this if uSky have custom SH soluton later on

		if (!m_StarsMesh) 
		{
			m_StarsMesh = new Mesh ();
			Star.InitializeStarfield ( ref m_StarsMesh);
		}

	#if !UNITY_EDITOR
		// fix the stars shader issue for WebGL and Android build
		if( Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android )
			StarTabUniform ();
	#endif 

		if(instance && instance != this)
			Debug.LogErrorFormat("Unexpected: uSkyPro.instance already set (to: {0}). Still overriding with: {1}.", instance.name, name);
		
		instance = this;

		uSkyInternal.UpdateAtmosphereEvent.AddListener (UpdateMaterialUniform);
		uSkyInternal.UpdatePrecomputedEvent.AddListener (UpdatePrecomputeData);
	}

	void OnDisable() 
	{
		uSkyInternal.UpdateAtmosphereEvent.RemoveListener (UpdateMaterialUniform);
		uSkyInternal.UpdatePrecomputedEvent.RemoveListener (UpdatePrecomputeData);

		instance = null;
	}

	void Start ()
	{
		UpdatePrecomputeData ();

		if (SkyboxMaterial != null && AutoApplySkybox)
			RenderSettings.skybox = SkyboxMaterial;
		else
			if (SkyboxMaterial == null)
				Debug.LogWarning ("Can't find uSkyboxPro Material");
	}

	void Update () 
	{
		// Set and check state and determines if it is dirty
		uSkyInternal.SetAtmosphereParameterState (this);

		// Draw Star field
		if ( m_StarsMesh && m_uStarsMaterial && StarIntensity > 0.1 )
			Graphics.DrawMesh (m_StarsMesh, Vector3.zero, Quaternion.identity, m_uStarsMaterial, 0 );
	}

	void OnDestroy () 
	{
		if (m_StarsMesh) 
			DestroyImmediate(m_StarsMesh);

		if (uSkyPrecomputeUtil.Transmittance2D)
			DestroyImmediate (uSkyPrecomputeUtil.Transmittance2D);
		if (uSkyPrecomputeUtil.Inscatter2D)
			DestroyImmediate (uSkyPrecomputeUtil.Inscatter2D);

		uSkyInternal.RemoveAllEventListeners ();

		if (RenderSettings.skybox == SkyboxMaterial)
			RenderSettings.skybox = null;
	}

	// Update is called by UpdatePrecomputedEvent
	void UpdatePrecomputeData ()
	{
		uSkyPrecomputeUtil.StartPrecompute (m_PrecomputeMaterial, precomputeParams);
	}

	// Too expensive to update at runtime, especially for mobile platform
	// So only update at Start
//	void UpdateAmbientSH ()
//	{
//		if (RenderSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Skybox /* && Application.isEditor */ )
//			DynamicGI.UpdateEnvironment ();
//	}

	// Update is called by UpdateAtmosphereEvent
	public void UpdateMaterialUniform ()
	{	
		Shader.SetGlobalFloat ("_uSkyExposure", Exposure );
		Shader.SetGlobalFloat ("_uSkyMieG", SunAnisotropyFactor);
		Shader.SetGlobalFloat ("_uSkyMieScale", MieScattering);
		Shader.SetGlobalVector ("_SunDirSize", SunDirectionAndSize());
		Shader.SetGlobalVector ("_MoonDirSize",	MoonDirectionAndSize());
		Shader.SetGlobalVector ("_uSkyNightParams", new Vector3(NightFade(), MoonFade(), NightFade() * OuterSpaceIntensity));
		Shader.SetGlobalVector ("_NightZenithColor", NightZenithColor * NightTimeBrightness() * 0.25f);// * Mathf.Lerp(0, 0.25f, NightTime));
		Shader.SetGlobalVector ("_NightHorizonColor", NightHorizonColor * NightFade() * 0.5f);
		Shader.SetGlobalVector ("_MoonInnerCorona", new Vector4(MoonInnerCorona.r * MoonFade(),
																MoonInnerCorona.g * MoonFade(),
																MoonInnerCorona.b * MoonFade(),
										      					4e2f / MoonInnerCorona.a)); 
		Shader.SetGlobalVector ("_MoonOuterCorona",new Vector4 (MoonOuterCorona.r * MoonFade() * 0.25f,
																MoonOuterCorona.g * MoonFade() * 0.25f,
																MoonOuterCorona.b * MoonFade() * 0.25f,
				                                                4 / MoonOuterCorona.a));
		Shader.SetGlobalFloat ("_StarIntensity", StarIntensity * 5f);

//		Shader.SetGlobalVector ("_uSkyGroundColor", GroundColor);
		Shader.SetGlobalFloat ("_uSkyGroundOffset", GroundOffset * (float)InscatterAltitudeSample);
		Shader.SetGlobalFloat ("_uSkyAltitudeScale", AltitudeScale);

		// Local material uniform
		if (SkyboxMaterial != null) 
			SkyboxMaterial.SetInt("_uSkySkyboxOcean", DisableSkyboxOcean? 0 : 1 );

		// AtmosphereImageEffect material
		Shader.SetGlobalVector ("_NightHorizonColorDeferred", NightHorizonColor * 0.5f * NightFade() / Mathf.Max ( Mathf.Pow (AtmosphereThickness, 0.25f), 1));

		// Update Reflection Probe
		uSkyInternal.MarkProbeStateDirty();

//		Debug.Log ("Material Uniforms updated!");
	}

	private void UpdateKeywords () 
	{
		// global shader keywords
		if(HDRMode) 
			Shader.EnableKeyword("USKY_HDR_MODE");
		else
			Shader.DisableKeyword("USKY_HDR_MODE");

		if (InscatterAltitudeSample != DepthSample.X1)
			Shader.EnableKeyword("USKY_MULTISAMPLE");
		else
			Shader.DisableKeyword("USKY_MULTISAMPLE");
	}

	private Vector4 SunDirectionAndSize ()
	{
		return m_Sun ? new Vector4 (-m_Sun.transform.forward.x,-m_Sun.transform.forward.y,-m_Sun.transform.forward.z, SunSize):
						new Vector4(0.321f,0.766f,-0.557f, SunSize); 
	}

	private Vector4 MoonDirectionAndSize ()
	{
		return m_Moon ? new Vector4 (-m_Moon.transform.forward.x, -m_Moon.transform.forward.y, -m_Moon.transform.forward.z, 8f / MoonSize) :
						new Vector4 (0.03261126f, -0.9445618f, -0.3267102f, 8f / MoonSize);
	}

	// DayTime : Based on Bruneton's uMuS Non-Linear function ( modified )
//	public float DayTimeBrightness ()
//	{ 
//		return Mathf.Clamp01 (Mathf.Atan (Mathf.Max (SunDirection.y, -0.1975f) * 5.35f) / 1.1f + 0.739f);
//	}

	// DayTime : Based on Bruneton's uMuS Linear function ( modified )
	public float DayTimeBrightness ()
	{ 
		return Mathf.Clamp01 (Mathf.Max (SunDirectionAndSize().y + 0.2f, 0.0f) / 1.2f);
	}

	public float NightTimeBrightness ()
	{ 
		return 1 - DayTimeBrightness(); 
	}

	public float NightFade ()
	{ 
		return NightTimeBrightness() * NightTimeBrightness() * NightTimeBrightness() * NightTimeBrightness();  // eq : power of 4
	} 

	public float MoonFade ()
	{ 
		return (MoonDirectionAndSize().y > 0.0)? Mathf.Max ( Mathf.Clamp01 ((MoonDirectionAndSize().y - 0.1f) * Mathf.PI)* NightTimeBrightness() - DayTimeBrightness(), 0f): 0f;  
	}

	void OnValidate () 
	{
		Exposure			= Mathf.Max (Exposure, 0f);
		MieScattering		= Mathf.Max (MieScattering, 0f);
		SunAnisotropyFactor	= Mathf.Clamp (SunAnisotropyFactor, 0f, 0.9995f);
		GroundOffset		= Mathf.Max (GroundOffset, 0f);
		AltitudeScale		= Mathf.Max (AltitudeScale, 0f);

		float wavelengthX	= Mathf.Clamp (Wavelengths.x, 380f, 780f);
		float wavelengthY	= Mathf.Clamp (Wavelengths.y, 380f, 780f);
		float wavelengthZ	= Mathf.Clamp (Wavelengths.z, 380f, 780f);
		Wavelengths = new Vector3 (wavelengthX, wavelengthY, wavelengthZ);

		// keep this update in editor
		if (instance == this)
		{
			SetEnumsAndToggles ();
			UpdateKeywords();

			if (SkyboxMaterial && AutoApplySkybox)
				RenderSettings.skybox = SkyboxMaterial;
		}
	}

	private readonly Vector2[] tabArray = 
	{
		new Vector2 (0.897907815f,-0.347608525f), new Vector2 (0.550299290f, 0.273586675f),
		new Vector2 (0.823885965f, 0.098853070f), new Vector2 (0.922739035f,-0.122108860f),
		new Vector2 (0.800630175f,-0.088956800f), new Vector2 (0.711673375f, 0.158864420f),
		new Vector2 (0.870537795f, 0.085484560f), new Vector2 (0.956022355f,-0.058114540f)
	};
	
	void StarTabUniform ()
	{
		for (int i = 0; i < tabArray.Length; i++) {
			string tab = "_tab" + i;
			m_uStarsMaterial.SetVector(tab, tabArray[i]);
		}
		m_uStarsMaterial.EnableKeyword("ENABLE_STARS_FIX");
	}
/*
	// Debug: show the precomputed texture buffers on game view
	void OnGUI (){
		if(uSkyPrecomputeUtil.Transmittance2D)
			GUI.DrawTexture (new Rect (0, 0, 256, 64), uSkyPrecomputeUtil.Transmittance2D);
		if(uSkyPrecomputeUtil.Inscatter2D)
			GUI.DrawTexture (new Rect (0, 68, 256, 128 * (int)precomputeParams.inscatterAltitudeSample), uSkyPrecomputeUtil.Inscatter2D, ScaleMode.ScaleToFit, false);
	}
*/

// Properties accessor --------------------
	#region Properties accessor
	public float Exposure {
		get { return exposure;}
		set { exposure = value;}
	}

	public float MieScattering {
		get { return mieScattering;}
		set { mieScattering = value;}
	}

	public float SunAnisotropyFactor {
		get { return sunAnisotropyFactor;}
		set { sunAnisotropyFactor = value;}
	}

	public float SunSize {
		get { return sunSize;}
		set { sunSize = value;}
	}

	//  Night Sky Settings ---------------------------------

	public NightModes NightMode {
		get { return nightSky.nightMode;}
		set { nightSky.nightMode = value;}
	}

	public Color NightZenithColor {
		get { return nightSky.nightZenithColor;}
		set { nightSky.nightZenithColor = value;}
	}

	public Color NightHorizonColor {
		get { return nightSky.nightHorizonColor;}
		set { nightSky.nightHorizonColor = value;}
	}

	public float StarIntensity {
		get { return nightSky.starIntensity;}
		set { nightSky.starIntensity = value;}
	}

	public float OuterSpaceIntensity {
		get { return nightSky.outerSpaceIntensity;}
		set { nightSky.outerSpaceIntensity = value;}
	}

	public Color MoonInnerCorona {
		get { return nightSky.moonInnerCorona;}
		set { nightSky.moonInnerCorona = value;}
	}

	public Color MoonOuterCorona {
		get { return nightSky.moonOuterCorona;}
		set { nightSky.moonOuterCorona = value;}
	}

	public float MoonSize {
		get { return nightSky.moonSize;}
		set { nightSky.moonSize = value;}
	}

	//	Other Settings -------------------------------------

	//	[Tooltip ("Ambient ground color lighting")]
	//	[SerializeField] 
	//	private Color groundColor = new Color (0.369f, 0.349f, 0.341f, 1); // temporary hide in inspector WIP
	//	public Color GroundColor {
	//		get { return groundColor;}
	//		set { groundColor = value;}
	//	}

	public float GroundOffset {
		get { return other.groundOffset;}
		set { other.groundOffset = value;}
	}

	public float AltitudeScale {
		get { return other.altitudeScale;}
		set { other.altitudeScale = value;}
	}

	public bool DisableSkyboxOcean {
		get { return other.disableSkyboxOcean;}
		set { other.disableSkyboxOcean = value; 
//			UpdateKeywords();
//			SkyboxMaterial.SetInt("_uSkySkyboxOcean",other.disableSkyboxOcean? 0 : 1 ); // not a keyword 
		}
	}

	public bool HDRMode {
		get { return other.HDRMode;}
		set { other.HDRMode = value; UpdateKeywords();}
	}

	//	Precompute Settings -------------------------

	public float AtmosphereThickness {
		get{ return precomputeParams.atmosphereThickness;}
		set{ precomputeParams.atmosphereThickness = value;}
	}

	public Vector3 Wavelengths {
		get{ return precomputeParams.wavelengths;}
		set{ precomputeParams.wavelengths = value;}
	}

	public Color SkyTint {
		get{ return precomputeParams.skyTint;}
		set{ precomputeParams.skyTint = value;}
	}

	public DepthSample InscatterAltitudeSample {
		get{ return precomputeParams.inscatterAltitudeSample;}
		set{ precomputeParams.inscatterAltitudeSample = value;}
	}

	#endregion
}

