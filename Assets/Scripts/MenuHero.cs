using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuHero : MonoBehaviour
{
    public Vector3 offset;
    public float smoothSpeed = 0.125f;

    private CreateMenuObject cmo;
    private void Start() {
        cmo = GameObject.Find("Points").GetComponent<CreateMenuObject>();

        if(cmo.getStatus() == "second")
            offset.x *= -1;
    }
    void FixedUpdate()
    {
        Vector3 desiredPosition = transform.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    void OnBecameInvisible(){
        
        cmo.createOnDie();
        Destroy(gameObject);
    }
}
