using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


//Photon Library
using Photon.Pun;

//other important Photon Library
using Photon.Realtime;


public class Launcher : MonoBehaviourPunCallbacks  //Allow us to acess and use all photon functions from the library
{
    public static Launcher instance;

    private void Awake()
    {
        instance = this;
    }

    [Header("Load Panel Settings")]
    public GameObject loadingScreen;
    public TMP_Text loadingText;

    [Header("Input User Name Settings")]
    public GameObject inputPanel;
    public TMP_InputField userNameInput;

    //static variable means, that if we change the scene and goes back to this scene, the value of this variable will be kept
    private static bool alreadyHasName;
    //if it were true before, when we return to this scene, it will be true, don´t work if we stop or close the game...
    
    [Header("Menu HUD")]
    public GameObject menuButtons;

    [Header("Create Room Settings")]
    public TMP_InputField roomNameInput;


    [Header("Joined Room Settings")]
    public GameObject roomScreen;
    public TMP_Text roomNameText;
   

    [Header("Error Screen Settings")]
    public GameObject errorScreen;
    public TMP_Text errorMensageText;

    [Header("Showing Current rooms Available")]
    public RoomButton roomButtonPrefab;
    public GameObject content;
    private List<RoomButton> allRoomsButtons = new List<RoomButton>();

    [Header("Miracle solution")]
    private Dictionary<string, RoomInfo> cachedRoomsList = new Dictionary<string, RoomInfo>();

    [Header("Name of the Players in the Room")]
    public TMP_Text playersNameText;
    public List<TMP_Text> allPlayersNameInTheCurrentRoom = new List<TMP_Text>();

    [Header("Starting Level Settings")]
    public string[] mapsAvailable;
    public GameObject startGameButton;

    [Header("Developer Settings")]
    public GameObject testButton;

    // Start is called before the first frame update
    void Start()
    {
        CloseMenus();

        loadingScreen.SetActive(true);
        loadingText.text = "Connecting to Network...";

        if(!PhotonNetwork.IsConnected)
        {
            //Tries to connect to Photon based on the settings we defined previously (like region, time, etc..)
            PhotonNetwork.ConnectUsingSettings();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //override means we are using a funcion of MonoBehaviourCallbacks and writing upside this function to do more things after
    //If you sucessed to connect to photon servers.
    public override void OnConnectedToMaster()
    {
        loadingText.text = "Joining Lobby...";

      
        //here we are trying to join lobbys, so we can access game rooms 
        PhotonNetwork.JoinLobby();

        //this update to all players that connected to photon web servers, the room that they should load in the future
        PhotonNetwork.AutomaticallySyncScene = true;


//Only if this is running in the unity editor game
#if UNITY_EDITOR
    testButton.SetActive(true);
#endif

    }

    //if you succeded to join game lobbys, to see game rooms
    public override void OnJoinedLobby()
    {
        CloseMenus();
        
        //if we already typed our name once at the inputfield
        if(!alreadyHasName)
        {
            inputPanel.SetActive(true);

            //if we already had saved one name before in our system
            if(PlayerPrefs.HasKey("playerName"))
            {
                //fill the input field with it
                userNameInput.text = PlayerPrefs.GetString("playerName");
            }
        }
        //if we already typed one name in game and saved this name, we set our name to web servers and return to main menu
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName"); 
            menuButtons.SetActive(true);  
        }
    }

   //function to create rooms and join then after creating
   public void CreateRoom()
   {
     //check if the input field is not empty
     if(!string.IsNullOrEmpty(roomNameInput.text))
     {
        //We create new a variable that access Photon Create room settings, so we can make our room settings 
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 3;

        //this function creates a room
        PhotonNetwork.CreateRoom(roomNameInput.text, options);

        loadingText.text = "Creating a Room...";
     }
   }

   //if you succeded to join the room you created
   public override void OnJoinedRoom()
   {
      CloseMenus();
      //To access information about your current room use: PhotonNetwork.CurrentRoom.the info you want (players, max players, room name, etc)
      roomNameText.text = "Welcome to " + PhotonNetwork.CurrentRoom.Name;
     
      roomScreen.SetActive(true); 

      //show all the players that are in our current room 
      ListAllPlayersInTheCurrentRoom();  

      //this verify if Iam is the host of this room, because only the host can starts the match
      VerifyTheHost();      
   } 

   //this will show to us all the players names that are in the same room as we
   private void ListAllPlayersInTheCurrentRoom()
   {
    //will erase all the previous texts that existed before at the scrollview
      foreach(TMP_Text texts in allPlayersNameInTheCurrentRoom)
      {
        Destroy(texts.gameObject);
      }
      allPlayersNameInTheCurrentRoom.Clear();

      //variable of type Player -> from library Photon.Realtime
      //create an array of Player type that will hold all the players that belongs to our current room
      Player[] players = PhotonNetwork.PlayerList;

      //loop to create texts based in the number of players that are inside our room
      for(int i = 0; i < players.Length; i++)
      {
        TMP_Text newText = Instantiate(playersNameText, playersNameText.transform.parent);
        
        //here we pass the name of the player inside the information of the array players to out newText that were just created
        newText.text = "" + players[i].NickName;
        newText.gameObject.SetActive(true);
        allPlayersNameInTheCurrentRoom.Add(newText);
      }
   }

   //here we will update to all of our players the listof players inside this room, if someone new joins it
   public override void OnPlayerEnteredRoom(Player newPlayer)
   {
        //we dont need a loop because we only want to add the information of the new player that entered
        //we keep the previous information in the scrollview 
        TMP_Text newText = Instantiate(playersNameText, playersNameText.transform.parent);
        
        //just passing the new player´s name that entered to the new text instantiated 
        newText.text = "" + newPlayer.NickName;
        newText.gameObject.SetActive(true);
        allPlayersNameInTheCurrentRoom.Add(newText);
   }

    //calling this function if someone left our current room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //we will erase all the players list name information, and update it again
        ListAllPlayersInTheCurrentRoom();
    }

    //case if we failed to create a room
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorMensageText.text = "failed to create room: " + message;
        CloseMenus();
        errorScreen.SetActive(true);
    }

    //you have to leave a room by photon
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        loadingScreen.SetActive(true);
        loadingText.text = "Leaving Room...";
    }

    //if you succeded to leave a room
    public override void OnLeftRoom()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }


//This is the part where the bug happened course error code ;(
//This function will get in realtime all the photon servers that are openned in this Lobby 
#region UpdateRoomsServers
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCachedRoomList(roomList);
    }
  
    //we use this function to get the rooms list, one by one, so then we can get their information( like it´s full,visible, available, etc)
    public void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        //here we going through all the opened rooms available in photon
        for (int i = 0; i < roomList.Count; i++)
        {
            //we store the information of an element of the list in a variable, so we can analize it
            RoomInfo info = roomList[i];

          
            //if the room is not avaliable or is full, we remove it from our dictionary, so it won´t be available to players see that room
            if (info.RemovedFromList || info.PlayerCount >= info.MaxPlayers)
            {
                cachedRoomsList.Remove(info.Name);
            }
            //otherwise, put this room information in our dictionarys
            else
            {
                cachedRoomsList[info.Name] = info;                
            }
        }
        RoomListButtonUpdate(cachedRoomsList);
    }

    //here we will create/delete room buttons, based on the information stored in our dictionary
    void RoomListButtonUpdate(Dictionary<string, RoomInfo> cachedRoomList)
    {
       
       //here we destroy all the room buttons in our List of buttons 
       foreach(RoomButton rb in allRoomsButtons)
       {
            Destroy(rb.gameObject);
       }
       allRoomsButtons.Clear();
 
       roomButtonPrefab.gameObject.SetActive(false);
        
        //for every RoomInformation inside in our dictionary
        foreach (KeyValuePair<string, RoomInfo> roomInfo in cachedRoomList)
        {
            //we create a new button to pass the Room information inside this element of the dictionary to the button
            RoomButton newButton = Instantiate(roomButtonPrefab, content.transform);
            //here we call the RoomButton Script inside the button, to pass the information to the button
            newButton.passTheInformation(roomInfo.Value);
            newButton.gameObject.SetActive(true);
            allRoomsButtons.Add(newButton);
        }
    }
    
    #endregion UpdateRoomsServers
   

   //the function will be called by a room button, so we can join a room by it´s name and info
    public void JoinRoom(RoomInfo thisRoomInfo)
    {
       PhotonNetwork.JoinRoom(thisRoomInfo.Name);
       CloseMenus();
       loadingText.text = "Joining this room";
       loadingScreen.SetActive(true);

    }

   

    void CloseMenus()
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
      
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    //this will set out NickName at the photon server
    public void setName()
    {
        //if the inputfield is not empty or null
        if(!string.IsNullOrEmpty(userNameInput.text))
        {
            //set player name to photon servers
            PhotonNetwork.NickName = userNameInput.text;

            //saving the info, so we don´t have to keeping typing our name everytime we return to menu
            PlayerPrefs.SetString("playerName", userNameInput.text);

            CloseMenus();
            inputPanel.SetActive(false);
            menuButtons.SetActive(true);
            alreadyHasName = true;
        }

    }

    //only the master (host) can use this function, that will load the same scene to everyone that are in the current room
    public void StartGame()
    {
        //this locks the room
        //PhotonNetwork.CurrentRoom.IsOpen = false;
        //PhotonNetwork.CurrentRoom.IsVisible = false;

        int numberLevel = Random.Range(0, mapsAvailable.Length);

        //load the same scene to everyone
        PhotonNetwork.LoadLevel(mapsAvailable[numberLevel]);
    }

    //this function verify if Iam is the host of this room, because only the host can starts the match
    private void VerifyTheHost()
    {
      // If iam is the host of this room
      if(PhotonNetwork.IsMasterClient)
      {
        //the start button appears to me
        startGameButton.SetActive(true);
        return;
      }
      //otherwise if iam not the host, the start match button dissapears to me
      startGameButton.SetActive(false);
    }

    //this verifies if the host had left the room, then other player became the host
    public override void OnMasterClientSwitched(Player newMaster)
    {
        //in this case the start button will appear to next host
        VerifyTheHost();
    }

    //this is to just create a room fast to test
    public void CreateSimpleTestRoom()
    {
        CloseMenus();
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 3;

        //this function creates a room
        PhotonNetwork.CreateRoom("fase Teste", options);

        loadingScreen.SetActive(true);
        loadingText.text = "Creating a Room...";
    }
}
