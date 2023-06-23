using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleMove : MonoBehaviour
{
    public Vector3 offset;
    private void Update() {
        Vector3 nextPos = transform.position + offset;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, nextPos, Time.deltaTime);

        transform.position = smoothedPosition;
    }
}
