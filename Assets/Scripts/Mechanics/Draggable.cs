using UnityEngine;
using System.Collections;

public class Draggable : MonoBehaviour{


    public GameObject target;

    public GameObject bullet;
    public GameObject bulletSpawn;

    public Transform originReference;
    private Transform defaultTargetParent;

    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device;

    private Vector3 origin;
	public float forceConstant = 200;

    public float bulletForce = 2000;

	private bool returning = false;
	public float precision = 0.01f;

    private bool dragging = false;

	
	
	private void Start(){

        defaultTargetParent = target.transform.parent;
        origin = originReference.position;
        trackedObject = GetComponent<SteamVR_TrackedObject>();

    }

    private void Update(){

        device = SteamVR_Controller.Input((int)trackedObject.index);

        origin = originReference.position;
        target.transform.rotation = originReference.rotation;


        if (returning){

            dragging = false;

            target.GetComponent<Rigidbody>().AddForce (forceConstant*(origin - target.transform.position));
            target.GetComponent<Rigidbody>().velocity *= 0.9f;
			if (target.GetComponent<Rigidbody>().velocity.magnitude < precision &&
			    Vector3.Distance(target.transform.position, origin) < precision){
                target.GetComponent<Rigidbody>().AddForce(Vector3.zero);
                target.GetComponent<Rigidbody>().velocity = Vector3.zero;
                target.transform.position = origin;
                target.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
				returning = false;
                dragging = false;

            }
        }
        else
        {
            if (!dragging)
            {
                target.transform.position = originReference.position;
            }
        }

    }

    
    void OnTriggerStay(Collider col)
    {

        if (col.CompareTag("sling"))
        {
            //Grab Object
            if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                dragging = true;
                col.transform.GetComponent<Rigidbody>().isKinematic = true;
                col.transform.SetParent(gameObject.transform);

                col.transform.position = transform.position;
                col.attachedRigidbody.constraints = RigidbodyConstraints.None;

            }

            if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                col.attachedRigidbody.isKinematic = false;
                col.transform.SetParent(defaultTargetParent);

                returning = true;

                FireBullet();

            }
        }
    }
    

    public void FireBullet()
    {
        Vector3 forceApplied = bulletForce * (origin - target.transform.position);
        GameObject newBullet = (GameObject) Instantiate(bullet, bulletSpawn.transform.position, Quaternion.identity);
        newBullet.GetComponent<Rigidbody>().AddForce(forceApplied);

    }


}