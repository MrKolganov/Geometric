using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Hero : MonoBehaviour
{
    private bool isGround;
    private Rigidbody2D _heroRigidbody;
    public float JumpForce = 6f;

    private void Start() {
        _heroRigidbody = GetComponent<Rigidbody2D>();
        isGround = true;
    }

    private void Update() {
        
        if(Input.GetButtonDown("Jump") && isGround)
        {
            _heroRigidbody.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
            
            isGround = false;
        }

        if(!isGround){
            transform.Rotate(0f, 0f, -2f);
        }

    }

    private void OnCollisionEnter2D(Collision2D other) {

        if(other.gameObject.tag == "Ground")
            isGround = true;

        if(other.gameObject.tag == "Obstacle")
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
    }

    void OnBecameInvisible(){
        Invoke("reloadScene", 1.2f);// задержка перед перезагрузкой сцены
    }
    
    void reloadScene(){
         SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
