using UnityEngine;
using System.Collections;

public class HumanoidAnimTriggers : MonoBehaviour {

    public SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;
    private Animator animator;


	void Awake() {

        animator = GetComponent<Animator>();
	}

    void Start()
    {
        //trackedObject = GetComponent<SteamVR_TrackedObject>();
    }

    void Update()
    {

        device = SteamVR_Controller.Input((int)trackedObject.index);
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            animator.SetTrigger("PhoneResponse");
        }

   }



}
