using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;


public class ChatSystem : MonoBehaviour
{
    public static ChatSystem instance;

    void Awake()
    {
        instance = this;    
    }

    [Header("Scroll view Settings")]
    public GameObject contentReference;
    public TMP_InputField inputMessage;

    [Header("Prefab text model")]
    public TMP_Text textChatModel;

    [Header("List of Messages")]
    public List<TMP_Text> messagesList = new List<TMP_Text>();
    public Scrollbar sliderOfMessages;



    public void UpdateChatMessages(string name, string message)
    {
        if(!string.IsNullOrEmpty(message))
        {
            
        TMP_Text newText = Instantiate(textChatModel, contentReference.transform);
        newText.text = "" + name + ": " + message;

        newText.gameObject.SetActive(true);
        messagesList.Add(newText);
        inputMessage.text = "";
        inputMessage.ActivateInputField();
        }
        
        if(sliderOfMessages.gameObject.activeSelf || messagesList.Count >= 4)
        {
           StartCoroutine(DelayMessage());
        }

        if(messagesList.Count >= 16)
        {
            ClearSomeChatMessages();
        }
    }

    void ClearSomeChatMessages()
    {
        int n = messagesList.Count;
        
        for(int i = 0; i < n; i++)
        {
            if(i < 13)
            {
                Destroy(messagesList[0].gameObject);
                messagesList.RemoveAt(0);
                
            }
        }
        
        sliderOfMessages.gameObject.SetActive(false);
    }

    IEnumerator DelayMessage()
    {
        yield return new WaitForSeconds(0.1f);
        sliderOfMessages.value = 0f;
    }
}
