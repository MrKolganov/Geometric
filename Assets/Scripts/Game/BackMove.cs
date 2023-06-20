using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackMove : MonoBehaviour
{
    
    void Update()
    {
        if(transform.position.x < -149f)
        {
            Destroy(gameObject);
        }
        else
        {
            transform.Translate(Vector3.left * Time.deltaTime * 4f);
        }
    }
}
