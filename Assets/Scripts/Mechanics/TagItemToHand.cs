using UnityEngine;
using System.Collections;

public class TagItemToHand : MonoBehaviour {

    public GameObject itemPrefab;
    private SteamVR_ControllerManager controllerManager;
    private bool isLeft;

    public Vector3 offsetPos = Vector3.zero;
    public Vector3 offsetRot = Vector3.zero;


    void Awake()
    {
        controllerManager = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<SteamVR_ControllerManager>();
    }


    void OnTriggerStay(Collider col)
    {
        if (col.gameObject == controllerManager.left || col.gameObject == controllerManager.right)
        {
            isLeft = (col.gameObject == controllerManager.left) ? true : false;
            SetItem();
            PositionItem();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Set Hands to activate item controller
    /// </summary>
    private void SetItem()
    {
        ((!isLeft) ? controllerManager.left : controllerManager.right).GetComponent<Draggable>().enabled = true;
        Destroy(((isLeft) ? controllerManager.left : controllerManager.right).GetComponent<Draggable>() );
    }

    /// <summary>
    /// Position item relative to triggered hand
    /// </summary>
    private void PositionItem()
    {
        GameObject mainItem = (GameObject)Instantiate(itemPrefab, ((isLeft) ? controllerManager.left : controllerManager.right).transform);
        mainItem.transform.localPosition = offsetPos;
        mainItem.transform.localEulerAngles = offsetRot;
    }

}
