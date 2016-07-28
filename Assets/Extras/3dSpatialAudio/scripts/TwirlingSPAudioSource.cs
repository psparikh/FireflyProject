#if UNITY_5_2 || UNITY_5_3 || UNITY_5_4
// The spatialization API is only supported by the final Unity 5.2 version and newer.
// If you get script compile errors in this file, comment out the line below.
#define ENABLE_SPATIALIZER_API
#endif

using UnityEngine;
using System.Collections;

public class TwirlingSPAudioSource : MonoBehaviour
{
    #if ENABLE_SPATIALIZER_API
	public enum options{ 
		NO = 0, 
		LIGHT = 1, 
		MEDIUM = 2,
		STRONG = 3
	}

    public bool EnableSpatialization = true;
	public options Reverb = options.LIGHT;
	public bool VirtualSpk = false;
    #endif

    void Start()
    {
    }

    void Update()
    {
        var source = GetComponent<AudioSource>();
        #if ENABLE_SPATIALIZER_API
		float reverbParam = (float) Reverb;
		source.SetSpatializerFloat(3, reverbParam);
		source.GetSpatializerFloat(3, out reverbParam); 
		float virtualSpkParam;
		if(VirtualSpk) virtualSpkParam = 1;
		else virtualSpkParam = 0;
		source.SetSpatializerFloat(4, virtualSpkParam);
		source.GetSpatializerFloat(4, out virtualSpkParam); 
        source.spatialize = EnableSpatialization;
        #endif
    }
}
