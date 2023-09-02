using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Photon Library
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks  //Allow us to acess and use all photon functions from the librarys
{
    [Header("Mouse Settings")]

    [Range(1f,6f)]
    public float mouseSensibility;
    private float verticalRoot;
    private Vector2 mouseInput;

    [Header("Up/Down look settings")]
    public Transform viewPoint;
    public bool invertedLook;
    private float invertvalue = -1f;

    [Header("Movement Settings")]
    [Range(1f,10f)]
    public float moveSpeed;
    private CharacterController charCon;
    private Vector3 moveDirection;
    private Vector3 movement;

    [Header("Jump Settings")]
    public float jumpForce;

    [Header("Gravity Settings")]
    private float yVelocity;
    private float gravityPushDown = 2.5f;

    [Header("Camera Reference")]
    public Camera cam;

    [Header("Animations Settings")]
    public Animator anim;


    void Start()
    {
        charCon = GetComponent<CharacterController>();

        //INVERT MOUSE SETTINGS
        if(invertedLook)
        {
            invertvalue = 1f; 
        }

        //HIDE MOUSE IN UNITY
        Cursor.lockState = CursorLockMode.Locked;

        //get the main camera reference
        cam = Camera.main;

        
        

    }

    // Update is called once per frame
    void Update()
    {
        //check if it were our input, otherwise it will move all players at the same time in the server
        if(photonView.IsMine)
        {

        //sychronize the bool "grounded" to grounded variable parameter inside animator
        anim.SetBool("grounded", charCon.isGrounded);
        //here we use magnitude, because if we are moving, magnitude will get the negative values and convert to a postive speed
        //so then we have a float value for our speed
        anim.SetFloat("speed", moveDirection.magnitude);

        
        #region First Person Look
        //FPS LOOK MOUSE
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensibility;

        //LOOK TO RIGHT AND LEFT
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,(transform.rotation.eulerAngles.y + mouseInput.x),transform.rotation.eulerAngles.z);
        
        // UP AND DOWN MOUSE
        verticalRoot += mouseInput.y;
        verticalRoot = Mathf.Clamp(verticalRoot, -60f, 60f);

        viewPoint.rotation = Quaternion.Euler(verticalRoot * invertvalue, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);

        #endregion First Person Look
        
        #region First Person Movement
        //PLAYER MOVEMENT IN X AND Z
        moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        //running settings
        if(Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = 8f;
        }
        else
        {
            moveSpeed = 5f;
        }

        //store my Y position in a variable, because at the next line, the y axis will be reset to zero
        yVelocity = movement.y;

    //get the blue arrow (z axis) and the red arrow (x axis) and multiplie by the player input, so the player move based on where he is facing
        movement = ((transform.forward * moveDirection.z) + (transform.right * moveDirection.x)).normalized * moveSpeed;

        //giving the y axis his previous value before reset
        movement.y = yVelocity;

        //if the character is touching the ground, the y axis won´t go lower than zero, so don´t push too much the character down
        if(charCon.isGrounded)
        {
            movement.y = 0f;
        }

        //Making JUMP, check if pressed space, and if the player is touching the ground
        if(Input.GetButtonDown("Jump") && charCon.isGrounded)
        {
            //give a small push on the player to go up
            movement.y = jumpForce;
        }
       
        //Appling gravity to the player everytime, to push him down, getting his y position and adding unity natural gravity
        // and multiplied by frames and the force to push him down
        movement.y += Physics.gravity.y * Time.deltaTime * gravityPushDown;

        //the position will be changed based on frames        
        charCon.Move(movement * Time.deltaTime);

        #endregion First Person Look

        #region Hide/Appear Mouse Cursor
        
        //Mouse appears if press escape, so you can close the game
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        //Hide the mouse if the user clicks at the game again, and if the mouse is appearing to the player.
         if(Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None && !UIController.me.optionsScreenHUD.activeInHierarchy)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        #endregion Hide/Appear Mouse Cursor

        //Chat system
        if(Input.GetKeyDown(KeyCode.Return))
        {
            string myMessage =  ChatSystem.instance.inputMessage.text;
            photonView.RPC("Chat",RpcTarget.AllBuffered, photonView.Owner.NickName, myMessage);
            
        }

        
        }
    }

    //CAMERA BEHAVIOUR
    //after Update, end of frame
    private void LateUpdate()
    {
        //check if it were our input, otherwise it will move all players at the same time in the server
        if(photonView.IsMine)
        {
            if( GameManager.instance.gameState != GameManager.GameStates.Ending)
            {
                //sync camera with view point
                cam.transform.position = viewPoint.transform.position;
                cam.transform.rotation = viewPoint.transform.rotation;
            }
            else
            {
                cam.transform.position = GameManager.instance.CameraPoint.transform.position;
                cam.transform.rotation = GameManager.instance.CameraPoint.transform.rotation;
            }
        }
        
        
    }


    //teleport the player to one of the spawn points
    private void respawnPlayer()
    {
        //sorteia uma posição para nascer
        int random = Random.Range(0, SpawnManager.instance.spawnPoints.Length);
        transform.position = SpawnManager.instance.spawnPoints[random].transform.position;
        transform.rotation = SpawnManager.instance.spawnPoints[random].transform.rotation;
    }

    //Chat System
    [PunRPC]
    public void Chat(string namePlayer, string myMessage)
    {
        ChatSystem.instance.UpdateChatMessages(namePlayer, myMessage);
    }
}
