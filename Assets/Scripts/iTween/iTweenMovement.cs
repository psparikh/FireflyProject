using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;
using UnityEngine.UI;

public class iTweenMovement : iTweenEditor
{

    public GameObject target;

    public Vector3[] pathArray;

    public bool pathOrient = false;

    [System.Serializable]
    public class OnStart : UnityEvent { };
    public OnStart onStart;

    [System.Serializable]
    public class OnComplete : UnityEvent { };
    public OnComplete onComplete;

    [System.Serializable]
    public class OnUpdate : UnityEvent { };
    public OnUpdate onUpdate;

    // Use this for initialization
    void Awake()
    {
        if (this.autoPlay)
            this.iTweenPlay();
    }

    public override void iTweenPlay()
    {
        Hashtable ht = new Hashtable();

        ht.Add("time", this.tweenTime);
        ht.Add("delay", this.waitTime);

        ht.Add("path", this.pathArray);

        ht.Add("looptype", this.loopType);
        ht.Add("easetype", this.easeType);

        ht.Add("orienttopath", pathOrient);

        if( pathOrient )
            ht.Add("lookahead", 0.001f );

        ht.Add("onstart", (Action<object>)(newVal => {
            if (onStart != null)
            {
                onStart.Invoke();
            }
        }));
        ht.Add("onupdate", (Action<object>)(newVal => {
            if (onUpdate != null)
            {
                onUpdate.Invoke();
            }
        }));
        ht.Add("oncomplete", (Action<object>)(newVal => {
            if (onComplete != null)
            {
                onComplete.Invoke();
            }
        }));

        ht.Add("ignoretimescale", ignoreTimescale);

        iTween.MoveTo(target, ht);
    }

    void OnDrawGizmos()
    {
        //Visual. Not used in movement
        iTween.DrawPath( this.pathArray );
    }

}