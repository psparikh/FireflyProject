using UnityEngine;
//using System;
using uSky;

[ExecuteInEditMode]
[HelpURL ("http://forum.unity3d.com/threads/uskypro-precomputed-atmospheric-scattering-2-0-beta.268288/")]
[AddComponentMenu("uSky/uSky Manager")]
public class uSkyManager : MonoBehaviour 
{
	public static uSkyManager instance { get; private set; }

	[Tooltip ("Update of the sky calculations in each frame.")]
	public bool SkyUpdate = true; // TODO: Update mode : Off, All_Settings, Timeline_Only
//	public bool useSlider = true;
	[Range(0.0f, 24.0f)][Tooltip ("This value controls the light vertically. It represents sunrise/day and sunset/night time( Rotation X )")]
	public float Timeline = 17.0f;

	[Range(-180.0f, 180.0f)][Space (5)][Tooltip ("This value controls the light horizionally.( Rotation Y )")]
	public float SunDirection = 0.0f;

	[Range(-60.0f, 60.0f)]
	public float NorthPoleOffset = 0.0f;

	[Space(10)][Tooltip ("This value sets the brightness of the sky.(for day time only)")]
	[Range(0.0f, 5.0f)]
	public float Exposure = 1.0f;

	[Range(0.0f, 5.0f)][Tooltip ("Rayleigh scattering is caused by particles in the atmosphere (up to 8 km). It produces typical earth-like sky colors (reddish/yellowish colors at sun set, and the like).")]
	public float RayleighScattering = 1.0f;

	[Range(0.0f, 5.0f)][Tooltip ("Mie scattering is caused by aerosols in the lower atmosphere (up to 1.2 km). It is for haze and halos around the sun on foggy days.")]
	public float MieScattering = 1.0f;

	[Range (0.0f,0.9995f)][Tooltip ("The anisotropy factor controls the sun's appearance in the sky.The closer this value gets to 1.0, the sharper and smaller the sun spot will be. Higher values cause more fuzzy and bigger sun spots.")]
	public float SunAnisotropyFactor = 0.76f;

	[Range (1e-3f,10.0f)][Tooltip ("Size of the sun spot in the sky")]
	public float SunSize = 1.0f;

	[Tooltip ("It is visible spectrum light waves. Tweaking these values will shift the colors of the resulting gradients and produce different kinds of atmospheres.")]
	// Wavelengths for visible light ray from 380 to 780 
	public Vector3 Wavelengths = new Vector3(680f, 550f, 440f); // sea level mie

	[Tooltip ("It is wavelength dependent. Tweaking these values will shift the colors of sky color.")]
	public Color SkyTint = new Color(0.5f, 0.5f, 0.5f, 1f);

	[Tooltip ("It is the bottom half color of the skybox")]
	public Color GroundColor = new Color(0.369f, 0.349f, 0.341f, 1f);


	[Tooltip ("It is a Directional Light from the scene, it represents Sun Ligthing")]
	public GameObject SunLight = null;
	

	public enum NightModes
	{
		Off = 1,
		Static = 2,
		Rotation = 3
	}
	[Space (10)]
	public NightModes NightSky = NightModes.Static;

//	[Tooltip ("Toggle the Night Sky On and Off")]
	private bool EnableNightSky {
		get {
			return (NightSky == NightModes.Off)? false : true ;
		}
	}

	[Tooltip ("The zenith color of the night sky gradient. (Top of the night sky)")]
	public Gradient NightZenithColor = new Gradient()
	{
		colorKeys = new GradientColorKey[] {
			new GradientColorKey(new Color32(050, 071, 099, 255), 0.225f),
			new GradientColorKey(new Color32(074, 107, 148, 255), 0.25f),
			new GradientColorKey(new Color32(074, 107, 148, 255), 0.75f),
			new GradientColorKey(new Color32(050, 071, 099, 255), 0.775f),
		},
		alphaKeys = new GradientAlphaKey[] {
			new GradientAlphaKey(1.0f, 0.0f),
			new GradientAlphaKey(1.0f, 1.0f)
		}
	};
	[Tooltip ("The horizon color of the night sky gradient.")]
	public Color NightHorizonColor = new Color(0.43f,0.47f,0.5f,1f);

	[Range(0.0f, 5.0f)][Tooltip ("This controls the intensity of the Stars field in night sky.")]
	public float StarIntensity = 1.0f;

	[Range(0.0f, 2.0f)][Tooltip ("This controls the intensity of the Outer Space Cubemap in night sky.")]
	public float OuterSpaceIntensity = 0.25f;

	[Tooltip ("The color of the moon's inner corona. This Alpha value controls the size and blurriness corona.")]
	public Color MoonInnerCorona = new Color(1f, 1f, 1f, 0.5f);

	[Tooltip ("The color of the moon's outer corona. This Alpha value controls the size and blurriness corona.")]
	public Color MoonOuterCorona = new Color(0.25f,0.39f,0.5f,0.5f);

	[Range(0.0f, 1.0f)][Tooltip ("This controls the moon texture size in the night sky.")]
	public float MoonSize = 0.15f;

		
	[Range(-90.0f, 90.0f)]
	public float MoonPositionOffset = 0.0f;

	[Tooltip ("It is additional Directional Light from the scene, it represents Moon Ligthing.")]
	public GameObject MoonLight;
		
	[Tooltip ("It is the uSkybox Material of the uSky.")]
	public Material SkyboxMaterial;

	[SerializeField][Tooltip ("It will automatically assign the current skybox material to Render Settings.")]
	private bool _AutoApplySkybox = true;
	public bool AutoApplySkybox {
		get{ return _AutoApplySkybox; }
		set{
			if (value && SkyboxMaterial){
				if (RenderSettings.skybox != SkyboxMaterial) 
					RenderSettings.skybox = SkyboxMaterial;
			} 
			_AutoApplySkybox = value;
		}
	}

	[HideInInspector]
	public bool LinearSpace; //  Auto Detection

	[Tooltip ("Toggle it if the Main Camera is using HDR mode and Tonemapping image effect.")]
	public bool SkyboxHDR = false; 

	private Quaternion sunEuler;
	private Matrix4x4 moon_wtl;

	// NOTE: "Stars.Shader" need to be placed in Resources folder for mobile build!
	private Material starMaterial ;
	void CheckStarMaterial (){
		if (starMaterial == null) {
			starMaterial = new Material (Shader.Find ("Hidden/uSky/Stars"));
			starMaterial.hideFlags = HideFlags.DontSave;
		}
	}
	private Mesh m_starsMesh ;

	public void InitStarsMesh (){
		StarField Stars = new StarField();
		m_starsMesh = new Mesh ();
		Stars.InitializeStarfield(ref m_starsMesh);
		
	}
	void Awake (){
		if (SunLight == null)
			SunLight = GameObject.Find ("Directional Light");
		#if UNITY_EDITOR
		if (SunLight == null)
			Debug.Log("Please apply the <b>Directional Light</b> to uSkyManager");
		
		#endif	
		CheckStarMaterial ();
		if (EnableNightSky && m_starsMesh == null) {
			InitStarsMesh ();
		}
	}
	void OnEnable() {

		InitKeywords ();
		InitSunAndMoon();

		if( SkyboxMaterial != null )
			InitMaterial (SkyboxMaterial);
		
		AutoApplySkybox = _AutoApplySkybox;

		if(instance && instance != this)
			Debug.LogErrorFormat("Unexpected: uSkyManager.instance already set (to: {0}). Still overriding with: {1}.", instance.name, name);
		
		instance = this;
	}
	void Start (){
		#if !UNITY_EDITOR
		// fix the stars shader problem for WebGL or Android build
		if( Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android )
			StarTabUniform ();
		#endif 
	}
	void OnDestroy() {
		if (m_starsMesh)	DestroyImmediate(m_starsMesh);
		if (starMaterial)	DestroyImmediate(starMaterial);
	}
	void OnDisable (){
		instance = null;
	}
	private void detectColorSpace (){
//			LinearSpace = QualitySettings.activeColorSpace == ColorSpace.Linear;
		#if UNITY_EDITOR
			LinearSpace = UnityEditor.PlayerSettings.colorSpace == ColorSpace.Linear;		// Editor
		#endif
		#if UNITY_IPHONE || UNITY_ANDROID
			LinearSpace = false; // Gamma only on mobile
		#endif
		if( SkyboxMaterial != null )
			InitMaterial (SkyboxMaterial);
	}

	public float Timeline01 { get{ return Timeline / 24; }}

	void Update()
	{
		if (SkyUpdate) {
			// reset Timeline slider
			if (Timeline >= 24.0f)
				Timeline = 0.0f;

			if (Timeline < 0.0f)
				Timeline = 24.0f;

			// Update every frame for all shader Paramaters
			if (SkyboxMaterial != null) {
				InitSunAndMoon ();
				InitMaterial (SkyboxMaterial);
			}
		}

		#if UNITY_EDITOR
			detectColorSpace ();  
		#endif

		// Draw Star field
		if (EnableNightSky && m_starsMesh != null && starMaterial != null && SunDir.y < 0.2f)
			Graphics.DrawMesh (m_starsMesh, Vector3.zero, Quaternion.identity, starMaterial, 0 );
			
	}

	// rotate and align the sun direction with Timeline slider
	void InitSunAndMoon()
	{
		float t = Timeline * 360.0f / 24.0f - 90.0f;
		sunEuler = Quaternion.Euler(new Vector3 (0f,SunDirection, NorthPoleOffset)) * Quaternion.Euler(t,0f,0f);
		if (SunLight != null) 
			SunLight.transform.rotation = sunEuler;

		if (NightSky == NightModes.Rotation && MoonLight != null) {
			Quaternion moonEuler = sunEuler * Quaternion.Euler (new Vector3 (180f, MoonPositionOffset, 180f));
			MoonLight.transform.rotation = moonEuler;
		}
	}
	public void InitMaterial(Material mat)
	{
		// SetGlobal
		Shader.SetGlobalVector ("_SunDir", SunDir); 
		Shader.SetGlobalVector ("_MoonDir", -getMoonMatrix.GetColumn(2));
		Shader.SetGlobalMatrix ("_Moon_wtl", getMoonMatrix);
		Shader.SetGlobalVector ("_betaR", betaR_RayleighOffset);
		Shader.SetGlobalVector ("_betaM", BetaM);

		// x = Sunset, y = Day, z = Night 
		Shader.SetGlobalVector ("_SkyMultiplier", skyMultiplier);

		mat.SetFloat ("_SunSize", 32.0f / SunSize);
		mat.SetVector ("_mieConst", mieConst);
		mat.SetVector ("_miePhase_g", miePhase_g);
		mat.SetVector ("_GroundColor", bottomTint);

		mat.SetVector ("_NightHorizonColor", getNightHorizonColor);
		mat.SetVector ("_NightZenithColor", getNightZenithColor);
		mat.SetVector ("_MoonInnerCorona", getMoonInnerCorona);
		mat.SetVector ("_MoonOuterCorona", getMoonOuterCorona); 
		mat.SetFloat ("_MoonSize", MoonSize);
		mat.SetVector ("_colorCorrection", ColorCorrection);

		if ( NightSky == NightModes.Rotation)
			mat.SetMatrix ("rotationMatrix", getMoonWorldToLocalMatrix);
		else
			mat.SetMatrix ("rotationMatrix", Matrix4x4.identity);

		mat.SetFloat ("_OuterSpaceIntensity", OuterSpaceIntensity);
		if (starMaterial != null) {
			starMaterial.SetFloat ("StarIntensity", starBrightness);
			if ( NightSky == NightModes.Rotation)
				starMaterial.SetMatrix ("rotationMatrix", getSunLocalToWorldMatrix );
			else
				starMaterial.SetMatrix ("rotationMatrix", Matrix4x4.identity );
		}

	}
	void InitKeywords (){
		Shader.DisableKeyword ("USKY_HDR_MODE");
		Shader.DisableKeyword ("NIGHTSKY_ON");

		if (SkyboxHDR) 
			Shader.EnableKeyword ("USKY_HDR_MODE");
		
		if (EnableNightSky)
			Shader.EnableKeyword ("NIGHTSKY_ON");
	}

	public Vector3 SunDir {
		get { return (SunLight != null)? SunLight.transform.forward * -1: new Vector3(0.321f,0.766f,-0.557f);}
	}

	private Matrix4x4 getMoonWorldToLocalMatrix {
		get { 
			return (MoonLight != null)? MoonLight.transform.worldToLocalMatrix : Matrix4x4.identity;
		}
	}
	private Matrix4x4 getSunLocalToWorldMatrix {
		get { 
			return (MoonLight != null)? MoonLight.transform.localToWorldMatrix : Matrix4x4.identity;
		}
	}
	private Matrix4x4 getMoonMatrix {
		get {
			if (MoonLight == null) {
					// predefined Moon Direction
					moon_wtl = Matrix4x4.TRS (Vector3.zero, new Quaternion (-0.9238795f, 8.817204e-08f, 8.817204e-08f, 0.3826835f), Vector3.one);
			} else if (MoonLight != null) {
					moon_wtl = MoonLight.transform.worldToLocalMatrix;
					moon_wtl.SetColumn (2, Vector4.Scale (new Vector4 (1, 1, 1, -1), moon_wtl.GetColumn (2)));
			}

			return moon_wtl;
		}
	}

	
	private Vector3 variableRangeWavelengths {
		get { 
			return new Vector3 (Mathf.Lerp ( Wavelengths.x + 150, Wavelengths.x - 150, SkyTint.r ),
			                    Mathf.Lerp ( Wavelengths.y + 150, Wavelengths.y - 150, SkyTint.g ),
			                    Mathf.Lerp ( Wavelengths.z + 150, Wavelengths.z - 150, SkyTint.b ));
		}
	}

	public Vector3 BetaR {
		get {
			// Evaluate Beta Rayleigh function is based on A.J.Preetham

			Vector3 WL = variableRangeWavelengths * 1e-9f;

			const float Km = 1000.0f;
			const float n = 1.0003f;		// the index of refraction of air
			const float N = 2.545e25f;		// molecular density at sea level
			const float pn = 0.035f;		// depolatization factor for standard air

			Vector3 waveLength4 = new Vector3 (Mathf.Pow (WL.x, 4), Mathf.Pow (WL.y, 4), Mathf.Pow (WL.z, 4));
			Vector3 theta = 3.0f * N * waveLength4 * (6.0f - 7.0f * pn);
			float ray = (8 * Mathf.Pow (Mathf.PI, 3) * Mathf.Pow (n * n - 1.0f, 2) * (6.0f + 3.0f * pn));
			return Km * new Vector3 (ray / theta.x, ray / theta.y, ray / theta.z) ;
		}
	}
	private Vector3 betaR_RayleighOffset{
		get{ return  BetaR * Mathf.Max (1e-3f, RayleighScattering); }
	}

	// Mie extinction : Constant value is based on Eric Bruneton
	private readonly Vector3 BetaM = new Vector3 (4e-3f,4e-3f,4e-3f) * 0.9f; 

	// 0 ~ 2.0 // Sun fall ratio function is based on Eric Bruneton 
	public float uMuS { get { return Mathf.Atan (Mathf.Max (SunDir.y, -0.1975f) * 5.35f) / 1.1f + 0.739f;}}

	// 0 ~ 1.0
	public float DayTime { get { return Mathf.Clamp01 (uMuS); }}

	public float SunsetTime {
		get { return Mathf.Clamp01 ((uMuS - 1.0f) * (1.5f / Mathf.Pow (RayleighScattering, 4f))); }
	}

	public float NightTime { get { return 1 - DayTime; }}
	
	public Vector3 miePhase_g {
		get{
			// partial mie phase : approximated with the Cornette Shanks phase function
			float g2 = SunAnisotropyFactor * SunAnisotropyFactor;
			float cs = LinearSpace && SkyboxHDR? 2f : 1f;
			return new Vector3 ( cs * ((1.0f - g2) / (2.0f + g2)), 1.0f + g2, 2.0f * SunAnisotropyFactor);
		}
	}
	public Vector3 mieConst {
		get { return new Vector3 (1f, BetaR.x/ BetaR.y, BetaR.x/ BetaR.z) * BetaM.x * MieScattering;}
	}

	// x = Sunset, y = Day, z = Night
	public Vector3 skyMultiplier {
		get{ return new Vector3 ( SunsetTime, Exposure * 4 * DayTime * Mathf.Sqrt(RayleighScattering), NightTime) ;}
	}

	private Vector3 bottomTint{
		get {
			float cs = LinearSpace ? 1e-2f : 2e-2f;
			return new Vector3 (betaR_RayleighOffset.x / (GroundColor.r * cs ),
			                    betaR_RayleighOffset.y / (GroundColor.g * cs ),
			                    betaR_RayleighOffset.z / (GroundColor.b * cs ));
//			float cs = LinearSpace ? 1f : 2f;
//			return new Vector3 ((GroundColor.r * cs ),
//			                    (GroundColor.g * cs ),
//			                    (GroundColor.b * cs ));

		}
	}

	public Vector2 ColorCorrection {
		get{
			return (LinearSpace && SkyboxHDR) ? new Vector2 (0.38317f, 1.413f): // (0.5f, 1.5f) :
				// using 2.0 instead of 2.2
				LinearSpace ? new Vector2 (1f, 2.0f) : Vector2.one; 
		}
	}

	public Color getNightHorizonColor{ get{ return NightHorizonColor * NightTime; }}

	public Color getNightZenithColor{ get{ return NightZenithColor.Evaluate(Timeline01) * 1e-2f; }}

	private float moonCoronaFactor {
		get{
			float m = 0.0f;
			float dir = (SunLight != null)? SunLight.transform.forward.y : 0.0f;
			if (NightSky == NightModes.Rotation)
				m = NightTime * dir;
			else
				m = NightTime;
			return m;
		}
	}

	private Vector4 getMoonInnerCorona {
		get {
			return new Vector4 (MoonInnerCorona.r * moonCoronaFactor,
			                    MoonInnerCorona.g * moonCoronaFactor,
			                    MoonInnerCorona.b * moonCoronaFactor,
			                    4e2f / MoonInnerCorona.a);
		}
	}

	private Vector4 getMoonOuterCorona {
		get {
			float cs = LinearSpace?  SkyboxHDR ? 16f : 12f: 8f;
			return new Vector4 (MoonOuterCorona.r * 0.25f * moonCoronaFactor,
			                    MoonOuterCorona.g * 0.25f * moonCoronaFactor,
			                    MoonOuterCorona.b * 0.25f * moonCoronaFactor,
			                    cs / MoonOuterCorona.a); 
		}
	}

	// Stars shader setting
	private float starBrightness {
		get {
			float cs = LinearSpace ? 1f : 1.5f;
			return StarIntensity * NightTime * cs;
		}
	}
	private readonly Vector2[] tab = 
	{
		new Vector2(0.897907815f,-0.347608525f),new Vector2(0.550299290f, 0.273586675f),
		new Vector2(0.823885965f, 0.098853070f),new Vector2(0.922739035f,-0.122108860f),
		new Vector2(0.800630175f,-0.088956800f),new Vector2(0.711673375f, 0.158864420f),
		new Vector2(0.870537795f, 0.085484560f),new Vector2(0.956022355f,-0.058114540f)
	};
	
	void StarTabUniform (){
		
		for (int i = 0; i < tab.Length; i++) {
			string tabArray = "_tab" + i;
			Shader.SetGlobalVector(tabArray,tab[i]);
		}
		
		Shader.EnableKeyword("ENABLE_STARS_FIX");
		
	}

	public void OnValidate() {

		Timeline = Mathf.Clamp (Timeline, 0f, 24f);
		SunDirection = Mathf.Clamp (SunDirection, -180.0f, 180.0f);
		NorthPoleOffset = Mathf.Clamp (NorthPoleOffset, -60f, 60f);
		SunAnisotropyFactor = Mathf.Clamp01 (SunAnisotropyFactor);
		Wavelengths.x = Mathf.Clamp (Wavelengths.x, 380f, 780f);
		Wavelengths.y = Mathf.Clamp (Wavelengths.y, 380f, 780f);
		Wavelengths.z = Mathf.Clamp (Wavelengths.z, 380f, 780f);

		if (enabled && base.isActiveAndEnabled) {
			Awake ();
			OnDisable ();
			OnEnable ();
		}
	}
}

