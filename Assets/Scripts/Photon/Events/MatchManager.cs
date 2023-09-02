using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
//

//Update server info (send and receive data) constantly
using Photon.Realtime; 

//to use Photon events
using ExitGames.Client.Photon; 

                            //to use realtime library  
public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{                                                       //to receive the events callbacks

    [Header("Singleton")]
    //singleton
    public static MatchManager instance;


    //instantiate in our game
    private void Awake()
    {
        instance = this;    
    }

    //Here we create the games situations (like turn based game), and we put as byte, because they will become codes, so we can call then by number
    public enum EventCodes : byte //is less expensive to web servers to comunitcate beyween each other, by sending bytes with each other
    {
        NewPlayer,  
        ListaAllPlayers,
        UpdateCharacterStatus,
        SendCurrentTime,
        NextMatch
    }


    [Header("List of players")]
    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    public int index;

    [Header("LeaderBoard settings")]
    public List<LeaderboardPlayer> lboardPlayers = new List<LeaderboardPlayer>();

    
    [Header("Next Match Settings")]
    public bool continuePlaying;
    

    // Start is called before the first frame update
    void Start()
    {
        if(!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("Menu");
        }   
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);
            GameManager.instance.gameState = GameManager.GameStates.Playing; 
        }  
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab) && GameManager.instance.gameState != GameManager.GameStates.Ending)
        {
            if(UIController.me.leaderboardHUD.activeInHierarchy)
            {
                UIController.me.leaderboardHUD.SetActive(false);
            }
            else
            {
                ShowLeaderBoard();
            }
        }
    }

    //this will be our event catcher, he will get all players events send
    public void OnEvent(EventData photonEvent)
    {
        
        //you can use 200 events in photon,after that is reserved, so we limit it
        if(photonEvent.Code < 200)
        {   
            //recieve event code and convert to enum(convert the byte again to its respective enum state)
            EventCodes eventCode = (EventCodes)photonEvent.Code;

            //object hold´s all kind of data(all type of variables)
            //here we get all information send, convert to array of objects and put inside an array called data, to store the send info
            object[] data = (object[])photonEvent.CustomData;

            Debug.Log("Received Event: " + eventCode);

            //verify the event code that were received
            switch (eventCode)
            {
                case EventCodes.NewPlayer:
                NewPlayerReceive(data);
                break;

                case EventCodes.ListaAllPlayers:
                ListPlayerReceive(data);
                break;

                case EventCodes.UpdateCharacterStatus:
                UpdateCharacterStatusReceive(data);
                break;

                case EventCodes.SendCurrentTime:
                ReceiveTimeForEveryone(data);
                break;

                case EventCodes.NextMatch:
                NextMatchReceive();
                break;
            }
        }
        
    }

#region OnEnable/Disable

    //when activates this object, we say the server that he will receive all players event send
    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    //when this object is disabled, we remove this object as callback receiver
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
#endregion

//---------------------------------------EVENTS ---------------------------
#region Event: NewPlayer
    //sending new player information to server, the parameter it´s the name of who send this information to the server
    public void NewPlayerSend(string nickName)
    {
        //create an array of data, that holds the respective player information
        object[] dataToSend = new object[4];
        dataToSend[0] = nickName;
        dataToSend[1] = PhotonNetwork.LocalPlayer.ActorNumber; //his ID in the game
        dataToSend[2] = 0;
        dataToSend[3] = 0;

        //create a new event to network
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer, //convert the respective enum event to byte event - first parameter
             dataToSend,                // the data that we want to send - second parameter
             new RaiseEventOptions{Receivers = ReceiverGroup.MasterClient},  //who will receive this event, this case the master receive - third parameter  
             new SendOptions{Reliability = true}                             //if the message is clean or crypted - four parameter
             );
    }


    //receiving player informarion by the event, it´s data, so we use array of objects
    public void NewPlayerReceive(object[] dataReceived)
    {
        //create a new player class, that will hold the information send by data of another one at the server
        PlayerInfo anotherPlayer = new PlayerInfo((string)dataReceived[0],(int)dataReceived[1],(int)dataReceived[2],(int)dataReceived[3]);
                                                    //as long as all values are type "object", we had to convert to there specific type in the class
        
        //add the new player class to a list
        allPlayers.Add(anotherPlayer);

        //call to all players the event to update the players in the lobby 
        ListPlayerSend();
        SendTimerToEveryone(GameManager.instance.CurrentTime);
    }
#endregion

#region Event: ListPlayerSend

    //this function its called by the master client, then everyone in the game receive the list of players in the room
    public void ListPlayerSend()
    {
        object[] dataToSend = new object[allPlayers.Count];

        for (int i = 0; i < allPlayers.Count; i++)
        {
            object[] pieces = new object[4]{allPlayers[i].name, allPlayers[i].playerID, allPlayers[i].kills, allPlayers[i].deaths};

            dataToSend[i] = pieces;
        }

         PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ListaAllPlayers,
            dataToSend,
            new RaiseEventOptions{Receivers = ReceiverGroup.All},
            new SendOptions {Reliability = true}
        );
    }

    public void ListPlayerReceive(object[] dataReceived)
    {
        allPlayers.Clear();        
 
        for (int i = 0; i < dataReceived.Length; i++)
        {
            object[] pieceOfPlayer = (object[])dataReceived[i]; //player piece is being stored
            PlayerInfo player = new PlayerInfo((string)pieceOfPlayer[0], (int)pieceOfPlayer[1], (int)pieceOfPlayer[2], (int)pieceOfPlayer[3]);
            allPlayers.Add(player);

            if(PhotonNetwork.LocalPlayer.ActorNumber == player.playerID)   //referencing ourselves
            {
                index = i;
            }
        }
 
    }
#endregion

#region CharacterStatus
    //UpdateCharacterStatus Event
    public void UpdateCharacterStatusSend(int actorSending, int arrayNumberElement, int amountToChange)
    {
        object[] dataToSend = new object[] { actorSending, arrayNumberElement, amountToChange};

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.UpdateCharacterStatus,
            dataToSend,
            new RaiseEventOptions{Receivers = ReceiverGroup.All},
            new SendOptions {Reliability = true}
        );
    }

    public void UpdateCharacterStatusReceive(object[] dataReceived)
    {
        int actor = (int)dataReceived[0];
        int statType = (int)dataReceived[1];
        int amount = (int)dataReceived[2];

        for(int i = 0; i < allPlayers.Count; i++)
        {
            if(allPlayers[i].playerID == actor)
            {
                switch(statType)
                {
                    case 0: //kills
                    allPlayers[i].kills += amount;
                    UpdatePlayerHud(actor, allPlayers[i].kills, allPlayers[i].deaths);
                    //call the game manager to verify if the game has ended
                    GameManager.instance.KillsCountVerify(allPlayers[i].kills);
                    break;

                    case 1: //deaths
                    allPlayers[i].deaths += amount;
                    UpdatePlayerHud(actor, allPlayers[i].kills, allPlayers[i].deaths);
                    break;
                }
                break;
            }
            
        }
    }

    public void UpdatePlayerHud(int actor, int kills, int deaths)
    {
       if(actor == PhotonNetwork.LocalPlayer.ActorNumber)
       {
            UIController.me.killsInfoText.text = "Kills: " + kills;
            UIController.me.deathsInfoText.text = "Deaths: " + deaths;
       }
    }
#endregion

#region SendTimer
    public void SendTimerToEveryone(float currentTime)
    {
        object[] dataToSend = new object[1];
        dataToSend[0] = currentTime;

        PhotonNetwork.RaiseEvent(
        (byte)EventCodes.SendCurrentTime, //convert the respective enum event to byte event - first parameter
             dataToSend,                // the data that we want to send - second parameter
             new RaiseEventOptions{Receivers = ReceiverGroup.All},  //who will receive this event, this case the master receive - third parameter  
             new SendOptions{Reliability = true}                             //if the message is clean or crypted - four parameter
             );
    }

    public void ReceiveTimeForEveryone(object[] dataReceived)
    {
        GameManager.instance.CurrentTime = (float)dataReceived[0] + 1f;
    }
#endregion    

#region NextMatch

    public void NextMatchSend()
    {
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NextMatch,
            null,
            new RaiseEventOptions{Receivers = ReceiverGroup.All},
            new SendOptions {Reliability = true}
        );
    }

    public void NextMatchReceive()
    {
        GameManager.instance.gameState = GameManager.GameStates.Playing; //change game state
        UIController.me.endScreen.SetActive(false);
        UIController.me.leaderboardHUD.SetActive(false);

        //reset kills and deaths info class for every player in the match
        foreach (PlayerInfo player in allPlayers)
        {
            player.kills = 0;
            player.deaths = 0;
            GameManager.instance.CurrentTime = GameManager.instance.LevelTime; //reset timer 
            GameManager.instance.timerText.enabled = true;
        }

        //update the leaderboard info too
        ListPlayerSend();

        //tell the player spawner to respawn his character again.
        PlayerSpawner.instance.SpawnPlayer();
    }
#endregion


    public void ShowLeaderBoard()
    {
        UIController.me.leaderboardHUD.SetActive(true);

        //destroy the previous leaderboard
        foreach (LeaderboardPlayer lp in lboardPlayers)
        {
            Destroy(lp.gameObject);
        }
        lboardPlayers.Clear();

        //disable the model to everyone
        UIController.me.leaderboardPlayerDisplay.gameObject.SetActive(false);

        //for every class (player) inside our List class of players
        foreach (PlayerInfo player in allPlayers)
        {
            //create the new display player model
            LeaderboardPlayer newPlayerDisplay = Instantiate(UIController.me.leaderboardPlayerDisplay, UIController.me.leaderboardPlayerDisplay.transform.parent);

            //the information of this display it will be the information of the equivalent player inside the list of players classes
            newPlayerDisplay.SetDetails(player.name, player.kills, player.deaths);
            newPlayerDisplay.gameObject.SetActive(true);
            lboardPlayers.Add(newPlayerDisplay);
        }
    }

    //save lifer that remove the player class, if some player had left the room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int index = allPlayers.FindIndex(x => x.name == otherPlayer.NickName);
        if (index != -1)
        {
            allPlayers.RemoveAt(index);

        }
        ListPlayerSend();
    }


}

//-----------------------------------------------------------------------------------------------------------------------

[System.Serializable] // here we say that this class can appear at the Inspector
//here we create a separate class that hold´s player information
public class PlayerInfo
{
    //the atributes of this class
    public string name;
    public int playerID, kills, deaths;

    //here we say the parameters inside the () will be stored in this class variables
    public PlayerInfo(string _name, int _playerID , int _kills , int _deaths)
    {
        name = _name;
        playerID = _playerID;
        kills = _kills;
        deaths = _deaths;
    }

    //example: PlayerInfo variable = new PlayerInfo("Rodrigo", 011 , 0 , 3);
}
