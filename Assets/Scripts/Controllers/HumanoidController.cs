﻿using UnityEngine;
using System.Collections;

public class HumanoidController : MonoBehaviour {

    public enum ActorState
    {
        Idle,
        GazeUp,
        Respond
    }

    public ActorState currentActorState = ActorState.Idle;

    public Transform headRef;
    public Transform bodyRef;
    public Transform phoneRef;

    public Transform target;

    public float minRange;

    public float speed = 5.0f;

    // Use this for initialization
    void Start() {

        currentActorState = ActorState.Idle;

    }

    // Update is called once per frame
    void Update() {

        if (currentActorState == ActorState.GazeUp)
        {
            float distance = Vector3.Distance(headRef.position, target.position);

            Transform end = (distance <= minRange) ? target : phoneRef;
            Quaternion targetRotation = Quaternion.LookRotation(end.position - headRef.position);

            // Smoothly rotate towards the target point.
            headRef.rotation = Quaternion.Slerp(headRef.rotation, targetRotation, speed * Time.deltaTime);
        }

    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(headRef.position, minRange);
    }
}
