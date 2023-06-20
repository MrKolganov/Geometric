using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public void pauseScreen(int i){

        if(i == 0)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;

    }
}
