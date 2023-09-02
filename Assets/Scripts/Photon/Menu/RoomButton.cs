using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//We need to Update in real time the information of this button
using Photon.Realtime;

//this script will be attached to a button, so it will have the information of a room
public class RoomButton : MonoBehaviour
{
    public TMP_Text roomNameText;
    public TMP_Text roomNumberOfPlayersText;

    //variable that will hold the web room information of an element of a list of rooms
    private RoomInfo info;

    //public TMP_Text isOpenText;
    //public TMP_Text isVisibleText;

    //we use this function, because when we create a button, we will pass the web room information of the list to this button
    public void passTheInformation(RoomInfo photonInfo)
    {
        info = photonInfo;
        roomNameText.text = info.Name;
        roomNumberOfPlayersText.text = "Players: " + info.PlayerCount + "/" + info.MaxPlayers;
        //isOpenText.text = "" + info.IsOpen;
        //isVisibleText.text = "" + info.IsVisible;
    }

    //when we click the room button, we call the launcher and pass the roomInfo of this room, so we can join this room by it´s name and info
    public void JoinThisRoom()
    {
        Launcher.instance.JoinRoom(info);
    }

    
}
