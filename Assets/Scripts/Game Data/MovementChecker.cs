using UnityEngine;
using System;

public class MovementChecker : MonoBehaviour
{
    private Vector3 lastPos;

    void Start()
    {
        lastPos = transform.position;
    }

    void LateUpdate()
    {
        if (transform.position != lastPos)
        {
            Debug.Log($"Moved from {lastPos} to {transform.position}", this);
            Debug.Log(Environment.StackTrace);
            lastPos = transform.position;
        }
    }
}