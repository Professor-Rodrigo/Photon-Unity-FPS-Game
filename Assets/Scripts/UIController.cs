using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class UIController : MonoBehaviour
{
    //Creates a instance, which means is a model that contains all this script information to everybody(all others scripts)
    public static UIController me;

    [Header("Weapons HUD")]
    public TMP_Text overheatedText;
    public Slider weaponTempSlider;

    [Header("Death HUD")] 
    public GameObject deathScreen;
    public TMP_Text deathMessageText;

    [Header("Health HUD")] 
    public Slider healthSlider;

    [Header("Kills/Death HUD")]
    public TMP_Text killsInfoText, deathsInfoText;

    [Header("Leaderboard HUD")]
    public GameObject leaderboardHUD;
    public LeaderboardPlayer leaderboardPlayerDisplay;

    [Header("End Screen HUD")]
    public GameObject endScreen;

    [Header("Options Screen HUD")]
    public GameObject optionsScreenHUD;


    //create this script as a object, so it can be acess by everybody, without drag and drop references (called singleton)
    private void Awake()
    {
        me = this;    
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && GameManager.instance.gameState == GameManager.GameStates.Playing)
        {
            ShowHideOptions();
        }
    }

    public void ShowHideOptions()
    {
        if(!optionsScreenHUD.activeInHierarchy)
        {
            optionsScreenHUD.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        optionsScreenHUD.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void ReturnToMainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
