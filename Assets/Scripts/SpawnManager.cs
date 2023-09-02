using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    ///
    //USE STATIC SO YOU CAN CREATE THIS AS A CLASS BEFORE THE GAMES START
    public static SpawnManager instance;

    public Transform[] spawnPoints;


    void Awake()
    {
       instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //we do this to desactivate all objects from this array, one by one. 
        foreach(Transform spawn in spawnPoints)
        {
            spawn.gameObject.SetActive(false);
        }
        //*REMEMBER, Foreach it´s recommended to check elements of array and lists, less perfomance
        //"for" it´s recomended to gain numbers and reducing numbers
    }

   
}
