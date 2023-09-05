using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public GameObject goButton;
    // public Transform player;
    // public Vector3 offset;
    // public float smoothSpeed = 0.125f;

    // void FixedUpdate()
    // {
    //     Vector3 desiredPosition = player.position + offset;
    //     Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
    //     transform.position = smoothedPosition;
    // }
    private void Start() {
        Time.timeScale = 0f;
    }

    // public void go(){
    //     goButton.SetActive(false);
    //     Time.timeScale = 1f;
    // }
}
