using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Hero : MonoBehaviour
{
    public Server server;
    private bool isGround;
    private Rigidbody2D _heroRigidbody;
    public float JumpForce = 6f;
    private bool moveBool;
    public bool isMainPlayer;

    //public GameObject victoryScreen;

    private void Start() {
        _heroRigidbody = GetComponent<Rigidbody2D>();
        isGround = true;
        moveBool = false;
    }

    private void Update() {
        if(this.isMainPlayer)
        {
            this.HandleMovement();
        }
    }

    private void HandleMovement()
    {
        var targetPos = this.transform.position;
        if(Input.GetButtonDown("Jump"))
        {
            moveBool = true;
        }

        if(Input.GetButtonUp("Jump"))
        {
            moveBool = false;
        }

        if(moveBool && isGround)
        {
            _heroRigidbody.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
            isGround = false;
        }
        this.server.SyncPlayerState(this.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D other) {

        if(other.gameObject.tag == "Ground")
            isGround = true;

        if(other.gameObject.tag == "Obstacle")
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // if(other.gameObject.tag == "Victory")
        // {
        //     victoryScreen.SetActive(true);
        //     Time.timeScale = 0f;
        // }
        
    }

    void OnBecameInvisible(){
        Invoke("reloadScene", 1.2f);// задержка перед перезагрузкой сцены
    }
    
    void reloadScene(){
         SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
