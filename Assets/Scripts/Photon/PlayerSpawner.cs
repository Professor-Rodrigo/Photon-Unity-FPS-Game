using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class PlayerSpawner : MonoBehaviour
{
    //making this script a singleton, so every player can get access of it, without drag and drop or transform reference 
    public static PlayerSpawner instance;

    void Awake()
    {
        instance = this;
    }

    [Header("Player at the Resources folder")]
    public GameObject playerPrefab;

    [Header("Death Effect at the Resources folder")]
    public GameObject deathEffect;
    public float respawnTime = 5f;

    //this variable will hold the reference of the player that will be create in the game
    private GameObject player;


    // Start is called before the first frame update
    void Start()
    {
        //check if the player is connected to web, so it can be respawned
        if(PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

    public void SpawnPlayer()
    {
       //pick a random number of the spawn points list
        int random = Random.Range(0, SpawnManager.instance.spawnPoints.Length);
        //store the position and rotation of the choosen spawn point in a Transform variable
        Transform newTransform = SpawnManager.instance.spawnPoints[random];

        //we instantiate the player (need string name in the first argument, at the one of the choosen points to spawn picked earlier)
        player = PhotonNetwork.Instantiate(playerPrefab.name, newTransform.position, newTransform.rotation);
      

       
    }

    public void Die(string damager)
    {
        UIController.me.deathMessageText.text = "you were killed by " + damager;

        //Call the event and tell everyone to increase one of our players death
        MatchManager.instance.UpdateCharacterStatusSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);

        //this is to avoid the corrotine to do more than once
        if(player != null)
        {
            StartCoroutine(DelayDie());
        }
    }

    public IEnumerator DelayDie()
    {
        //create the death effect of this player in all games connected, everyone will see it
        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);

        PhotonNetwork.Destroy(player);

        player = null;

        //appear the death screen, telling who killed you
        UIController.me.deathScreen.SetActive(true);

        yield return new WaitForSeconds(respawnTime);

        UIController.me.deathScreen.SetActive(false);


        if(GameManager.instance.gameState == GameManager.GameStates.Playing && player == null)
        {
            //respawn him in another point of the game
            SpawnPlayer();
        }
       
    }
}
