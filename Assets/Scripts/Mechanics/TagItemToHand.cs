using UnityEngine;
using System.Collections;

public class TagItemToHand : MonoBehaviour {

    public GameObject mainItem;
    private SteamVR_ControllerManager controllerManager;
    private bool isLeft;

    void Awake()
    {
        controllerManager = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<SteamVR_ControllerManager>();
    }


    void OnTriggerStay(Collider col)
    {
        if (col.gameObject == controllerManager.left || col.gameObject == controllerManager.right)
        {
            isLeft = (col.gameObject == controllerManager.left) ? true : false;
            SetHands();
            PositionItem();
            mainItem.SetActive(true);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Set Hands to activate item controller
    /// </summary>
    private void SetHands()
    {
        ((!isLeft) ? controllerManager.left : controllerManager.right).GetComponent<Draggable>().enabled = true;
        Destroy(((isLeft) ? controllerManager.left : controllerManager.right).GetComponent<Draggable>() );
    }

    /// <summary>
    /// Position item relative to triggered hand
    /// </summary>
    private void PositionItem()
    {
        mainItem.gameObject.transform.SetParent(((isLeft) ? controllerManager.left : controllerManager.right).transform );
        mainItem.transform.localPosition = Vector3.zero;
        mainItem.transform.localEulerAngles = new Vector3(90.0f, 180.0f, 180.0f);
    }

}
