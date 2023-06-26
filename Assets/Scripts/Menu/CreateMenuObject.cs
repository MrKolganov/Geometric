using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMenuObject : MonoBehaviour
{
    public Transform firstPoint;
    public Transform secondPoint;

    public GameObject hero;
    private string status;

    private void Start() {
        Instantiate(hero, firstPoint);
        status = "first";
    }

    public void createOnDie(){ // создаёт hero при выходе из зоны видимости предыдущего 
        if(status == "first")
        {
            Instantiate(hero, secondPoint);
            status = "second";
        }
        else
        {
            Instantiate(hero, firstPoint);
            status = "first";           
        }
    }

    public string getStatus()
    {
        return status;
    }
}
