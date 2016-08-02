using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GrabObject : MonoBehaviour
{

    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;

    public float throwSpeed = 2.0f;

    // Use this for initialization
    void Start()
    {
        trackedObject = GetComponent<SteamVR_TrackedObject>();
    }

    // Update is called once per frame
    void Update()
    {
        device = SteamVR_Controller.Input((int)trackedObject.index);

        if (device.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void OnTriggerStay(Collider col)
    {
        if ( col.CompareTag("item") )
        {


            //Grab Object
            if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                col.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                col.gameObject.transform.SetParent(gameObject.transform);
            }

            /*
            if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                col.attachedRigidbody.isKinematic = false;
                col.gameObject.transform.SetParent(null);

                TossObject(col.attachedRigidbody);
            }
            */
        }
    }

    public void TossObject(Rigidbody rigidBody)
    {
        //Apply Force
        rigidBody.velocity = device.velocity * throwSpeed;
        rigidBody.angularVelocity = device.angularVelocity;
    }
}
