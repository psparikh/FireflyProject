using UnityEngine;
using System.Collections;

public class SlingshotSpawner : MonoBehaviour
{

    public GameObject slingshotPrefab;
    private SteamVR_ControllerManager controllerManager;
    private bool isLeft;
    private bool hasSpawned;

    private SteamVR_TrackedObject mainController;
    private SteamVR_TrackedObject pivotController;

    public Vector3 offsetPos = Vector3.zero;
    public Vector3 offsetRot = Vector3.zero;


    void Awake()
    {
        controllerManager = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<SteamVR_ControllerManager>();
    }

    void Update()
    {
        if( hasSpawned)
        {
            Destroy(gameObject);
        }
    }


    void OnTriggerStay(Collider col)
    {
        if (!hasSpawned)
        {
            if (col.gameObject == controllerManager.left || col.gameObject == controllerManager.right)
            {
                isLeft = (col.gameObject == controllerManager.left) ? true : false;
                SetDevices();
                SpawnItem();

                hasSpawned = true;
            }
        }
    }

    /// <summary>
    /// Set controllers for slingshot
    /// </summary>
    private void SetDevices()
    {
        mainController = ((isLeft) ? controllerManager.left : controllerManager.right).GetComponent<SteamVR_TrackedObject>();
        pivotController = ((!isLeft) ? controllerManager.left : controllerManager.right).GetComponent<SteamVR_TrackedObject>();
    }

    /// <summary>
    /// Spawn item
    /// </summary>
    private void SpawnItem()
    {
        GameObject slingshot = (GameObject)Instantiate(slingshotPrefab, mainController.transform);
        slingshot.transform.localPosition = offsetPos;
        slingshot.transform.localEulerAngles = offsetRot;
        slingshot.GetComponentInChildren<SleeveController>().trackedPivotObject = pivotController;
        slingshot.GetComponentInChildren<SleeveController>().trackedMainObject = mainController;
    }

}
