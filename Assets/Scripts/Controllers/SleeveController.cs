﻿using UnityEngine;
using System.Collections;

[RequireComponent( typeof( Follow ))]
public class SleeveController : MonoBehaviour
{

    private int bulletIndex;
    private int currentBulletIndex;

    public GameObject[] bulletPrefabs;
    public GameObject[] modelPrefabs;
    public GameObject bulletSpawnPoint;
    private GameObject bulletPlaceholder;

    private GameObject selectedBullet;

    private Transform sleeveParent;

    public SteamVR_TrackedObject trackedPivotObject;
    public SteamVR_TrackedObject trackedMainObject;

    private SteamVR_Controller.Device mainDevice;
    private SteamVR_Controller.Device pivotDevice;

    private Transform origin;

    public float forceConstant = 200;
    public float bulletForce = 2000;
    public bool returning = false;
    public bool dragging = false;
    public float precision = 0.01f;

    void Awake()
    {
        currentBulletIndex = 0;
        bulletIndex = currentBulletIndex;

        dragging = false;
        returning = false;

    }


    void Start()
    {
        sleeveParent = transform.parent;
        origin = GetComponent<Follow>().target.transform;

        ChangeBulletPlaceholder();
    }

    void Update()
    {
        //Control pivot hand
        if (trackedPivotObject != null)
        {

            pivotDevice = SteamVR_Controller.Input((int)trackedPivotObject.index);

            //origin = GetComponent<Follow>().target.transform.position;
            transform.rotation = origin.rotation;

            if (returning)
            {

                dragging = false;

                GetComponent<Rigidbody>().AddForce(forceConstant * (origin.position - transform.position));
                GetComponent<Rigidbody>().velocity *= 0.9f;

                //Vector3 toTarget = origin.position - transform.position;
                if (GetComponent<Rigidbody>().velocity.sqrMagnitude < precision
                    && Vector3.Distance( origin.position, transform.position) < precision)
                {
                    GetComponent<Rigidbody>().AddForce(Vector3.zero);
                    GetComponent<Rigidbody>().velocity = Vector3.zero;

                    GetComponent<Follow>().enabled = true;

                    returning = false;
                }
            }
            
        }

        //Control main hand
        if( trackedMainObject != null)
        {
            mainDevice = SteamVR_Controller.Input((int)trackedMainObject.index);

            if (mainDevice.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                bulletIndex = (bulletIndex + 1) % (bulletPrefabs.Length);
                ChangeBulletPlaceholder();
            }

        }

    }


    void OnTriggerStay(Collider col)
    {

        if (trackedPivotObject != null)
        {
            pivotDevice = SteamVR_Controller.Input((int)trackedPivotObject.index);

            //On Drag
            if (pivotDevice.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                returning = false;
                dragging = true;

                GetComponent<Follow>().enabled = false;

                transform.GetComponent<Rigidbody>().isKinematic = true;
                transform.SetParent(col.transform);

                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

            }

            //On Release
            if (pivotDevice.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                mainDevice = SteamVR_Controller.Input((int)trackedMainObject.index);

                GetComponent<Rigidbody>().isKinematic = false;
                transform.SetParent(sleeveParent);

                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

                returning = true;

                FireBullet();

                bulletIndex = Random.Range(0, bulletPrefabs.Length - 1);
                ChangeBulletPlaceholder();
            }
        }
    }

    /// <summary>
    /// Fire the bullet
    /// </summary>
    public void FireBullet()
    {
        Vector3 forceApplied = bulletForce * (origin.position - transform.position);
        GameObject newBullet = (GameObject)Instantiate(bulletPrefabs[currentBulletIndex], bulletSpawnPoint.transform.position, Quaternion.identity);
        newBullet.GetComponent<Rigidbody>().AddForce(forceApplied);

    }

    /// <summary>
    /// Update model of next bullet to be fired
    /// </summary>
    private void ChangeBulletPlaceholder()
    {
        if (currentBulletIndex != bulletIndex)
        {
            currentBulletIndex = bulletIndex;
            if (bulletPlaceholder) Destroy(bulletPlaceholder);
            bulletPlaceholder = (GameObject)Instantiate(modelPrefabs[currentBulletIndex], transform);
        }
    }


}