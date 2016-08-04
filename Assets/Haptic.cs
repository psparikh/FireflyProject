using UnityEngine;
using System.Collections;

public class Haptic : MonoBehaviour {

    public ushort pulsePower;
    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;
    private int count;

    void Start()
    {
        count = 0;
        trackedObject = GetComponent<SteamVR_TrackedObject>();
    }


    // Update is called once per frame
    void Update () {
        device = SteamVR_Controller.Input((int)trackedObject.index);
        device.TriggerHapticPulse(pulsePower);

        /* if (pulsePower > 0)
         {
             device.TriggerHapticPulse(pulsePower);
             count++;
         }

         if (count >= 50)
         {
             device.TriggerHapticPulse(0);
             count = 0;
         }*/
    }
}
