using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;

//Update server info (send and receive data) constantly
using Photon.Realtime; 

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Singleton")]
    //singleton
    public static GameManager instance;


    //instantiate in our game
    private void Awake()
    {
        instance = this;    
    }


    public enum GameStates
    {
        Waiting,
        Playing,
        Ending
    };

    [Header("End Settings")]
    public int killsLimit = 2;
    public float endingTime = 50f;
    public Transform CameraPoint;
    
    [Header("Game States")]
    public GameStates gameState = GameStates.Waiting;

    [Header("Time System")]
    public float LevelTime = 90f;
    public float CurrentTime;
    public TMP_Text timerText;


    private void Start()
    {
        CurrentTime = LevelTime;    
    }

    void Update()
    {
        //decrease the timer until reaches 1 second or the match is still running
        if(CurrentTime > 1f && gameState == GameStates.Playing)
        {
            CurrentTime -= Time.deltaTime;
       
            float minutes = Mathf.FloorToInt(CurrentTime / 60);
            float seconds = Mathf.FloorToInt(CurrentTime % 60);

            timerText.text = string.Format("{00:00} : {1:00}", minutes, seconds);
        }
        //otherwise, end the match
        else
        {
            StartCoroutine(EndingGame());
        }
        
    }


    //verifiy if this actor/player had reached the maximum kills needed to end the match
    public void KillsCountVerify(int kills)
    {
        if(kills >= killsLimit)
        {
            StartCoroutine(EndingGame());
        }
    }

    IEnumerator EndingGame()
    {
        if(gameState != GameStates.Ending)
        {
        timerText.enabled = false;
        gameState = GameStates.Ending;
        MatchManager.instance.ListPlayerSend(); //Update to everyone all the players that are in this room

        UIController.me.deathScreen.SetActive(false);
        UIController.me.optionsScreenHUD.SetActive(false);
        UIController.me.endScreen.SetActive(true);
           
        //if i´am the host, i destroy all the other players in the current room
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }

        MatchManager.instance.ShowLeaderBoard(); //show the leaderboard to everyone

        Cursor.lockState = CursorLockMode.None; //mouse appears again
        Cursor.visible = true;

        //teleport the camera to a specific point
        Camera.main.transform.position = CameraPoint.transform.position;
        Camera.main.transform.rotation = CameraPoint.transform.rotation;

        yield return new WaitForSeconds(endingTime);

        //if we decided to not continue playing
        if(!MatchManager.instance.continuePlaying)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        }
        //otherwise load next match
        else 
        {
            //if we are the master, then we will send event to everyone that a match will begin
            if(PhotonNetwork.IsMasterClient)
            {
                MatchManager.instance.NextMatchSend();

                int numberLevel = Random.Range(0, Launcher.instance.mapsAvailable.Length);

                //load the same scene to everyone
                PhotonNetwork.LoadLevel(Launcher.instance.mapsAvailable[numberLevel]);
                
            }
        }
        }
        
       
    }


    //this will be triggered when you left the current room
    public override void OnLeftRoom()
    {
        base.OnLeftRoom(); //to not bug our game

        SceneManager.LoadScene("Menu");
    } 

}
