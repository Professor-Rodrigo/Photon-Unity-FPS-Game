using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class ShootSystem : MonoBehaviourPunCallbacks  //Allow us to acess and use all photon functions from the librarys
{

    [Header("Camera Reference")]
    private Camera cam;

    [Header("Bullet Hole Reference")]
    [SerializeField] private GameObject bulletImpact;

    [Header("Player Blood Impact Reference")]
    public GameObject bloodImpact;

    [Header("Fire Cadency Settings")]
    private float shootCounter;

    [Header("Overheat Weapon Settings")]
    private float actualWeapoanHeat = 0f, maxWeaponHeat = 10f, HeatWeaponDecreaseAmount = 4f;
    private float OverheatWeaponDecreaseAmount = 6f;
    private bool weaponOverheated;

    [Header("Guns Settings")]
    [SerializeField] private Gun[] allGameGuns;
    private int currentGunWearing = 0; 

    [Header("Health Variables")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("First person model")]
    public GameObject playerModelToHide;
    public Transform modelGunPoint,gunHolder;

    [Header("Skins")]
    public Material[] allSkins;

    // Start is called before the first frame update
    void Start()
    {
        //get the main camera reference
        cam = Camera.main;

        //get the instance reference and access the max slider value to be the max weapon heat value
        UIController.me.weaponTempSlider.maxValue = maxWeaponHeat;

        currentHealth = maxHealth;

        //update to everyone the current weapon that i´am holding
        photonView.RPC("ChangeMyGunToEveryoneConnected", RpcTarget.All, currentGunWearing);
        
        //checking if it´s my player and game to the things
        if(photonView.IsMine)
        {
            //hide our player model in our game, but appears to everyone in their game
            StartCoroutine(DelayToHideThisModel());
            UIController.me.healthSlider.maxValue = currentHealth;
            UIController.me.healthSlider.value = currentHealth;
        }
        //otherwise teleport the gun holder to the others characters right hand
        else
        {
            gunHolder.parent = modelGunPoint;
            //local.position will be the position of the parent
            gunHolder.localPosition = Vector3.zero;
            //local rotation will be the rotation of the parent
            gunHolder.localRotation = Quaternion.identity;
        }

    //the player skin will be based on his actor number, so it will be one of the array materials, just change the renderer material character
        playerModelToHide.GetComponent<Renderer>().material = allSkins[photonView.Owner.ActorNumber];
       
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine)
        {

        #region Weapon Heat/Shoot
        allGameGuns[currentGunWearing].muzzleFlash.SetActive(false);

        //if the weapon reache´s his maximum heat, can´t shoot
        if(!weaponOverheated)
        {
            //Holding shoot button
            if(Input.GetMouseButton(0) && allGameGuns[currentGunWearing].isAutomatic)
            {
            //Decrease time, so you can shoot again
            shootCounter -= Time.deltaTime; 

                if(shootCounter <= 0f)
                {
                     shoot();
                } 
            }   
            //clicking shoot button
            if(Input.GetMouseButtonDown(0))
            {
                shoot();
            }   
            
            //freeze the weapon´s heat, decrease the bar
            actualWeapoanHeat -= HeatWeaponDecreaseAmount * Time.deltaTime;
        }
        else
        {
            //heat bar took´s more time to decrease
            actualWeapoanHeat -= OverheatWeaponDecreaseAmount * Time.deltaTime;

            if(actualWeapoanHeat < 0f)
            {
                weaponOverheated = false;

                //gets the UIController (instance or singleton) to call the variable inside him and manipulate it from here 
                UIController.me.overheatedText.gameObject.SetActive(false);

            }
        }
        
        //avoid the heat to go less than 0, no negative numbers
        if(actualWeapoanHeat < 0f)
        {
            actualWeapoanHeat = 0f;
        }

        //get the instance reference and acess the slider value to be the actual weapon heat value
        UIController.me.weaponTempSlider.value = actualWeapoanHeat;

        #endregion Weapon Heat/Shoot


        #region Switching Gun by Scroll Mouse
        //Scroll the mouse up, to change to the next weapon in the array
        if(Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            currentGunWearing++;
            
            //reach the last element of the array
            if(currentGunWearing >= allGameGuns.Length)
            {
                //reset to the variable to be the first element of the array
                currentGunWearing = 0;
            }

            photonView.RPC("ChangeMyGunToEveryoneConnected", RpcTarget.All, currentGunWearing);

        }
        //Scroll the mouse down, to change to the previous weapon in the array
         if(Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            currentGunWearing--;
           
           //reach the first element of the array
            if(currentGunWearing < 0)
            {
                //reset to the variable to be the last element of the array
                currentGunWearing = allGameGuns.Length - 1;
            }

            photonView.RPC("ChangeMyGunToEveryoneConnected", RpcTarget.All, currentGunWearing);

        }
        #endregion Switching Gun by Scroll Mouse

        }

    }



    private void shoot()
    {
        //create a Ray (laser) that goes forward, 5 to right and 5 up based on where camera is facing
        Ray ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, 0f));

        //the origin of the laser, will be where the camera is
        ray.origin = cam.transform.position;
        
        //if ray hits something it will store the thing that hited with a collider in a variable called hitThing
        if(Physics.Raycast(ray, out RaycastHit hitThing))
        {
            //check if the raycast hit something with the tag "Player" (raycast hit´s only things with a collider)
            if(hitThing.collider.gameObject.tag == "Player")
            {

                //create to all players the blood effect, where it hited in raycast, with 0 rotation in x,y,z
                PhotonNetwork.Instantiate(bloodImpact.name, hitThing.point, Quaternion.identity);

                //get the photon view of the player that we shoot and call the function "DealDamage" inside the player that we shoot to do damage to him
                hitThing.collider.gameObject.GetPhotonView().RPC("DealDamage", hitThing.collider.gameObject.GetPhotonView().Controller, photonView.Owner.NickName, allGameGuns[currentGunWearing].shotDamage, PhotonNetwork.LocalPlayer.ActorNumber); 
            }                                                                                                                           //pass our name             //get the damage info of our current gun                                                                        

            //otherwise, just create to only that player the normal bullet impact in his game
            else
            {
                //Create the bullet impact where it hited, and the rotation is where the object that receive the shoot is facing
                GameObject NewBulletImpact = Instantiate(bulletImpact, hitThing.point + (hitThing.normal* .002f), Quaternion.LookRotation(hitThing.normal, Vector3.up));
                                                    //The rotation parameters of the bullet: first the normal is the direction where the facing is looking
                                                    //, second -> and the height is a vector3(0,1,0)
                Destroy(NewBulletImpact, 10f);
            }
        }

        //reset the value based on the gun settings you wearing so you can shoot again (fire cadency)
        shootCounter = allGameGuns[currentGunWearing].timeBetweenShoots;


        //increase heat bar, based on how much heat you will gain depending of the weapon
        actualWeapoanHeat += allGameGuns[currentGunWearing].heatPerShoot;

        //check if overheated weapon
        if(actualWeapoanHeat >= maxWeaponHeat)
        {
            actualWeapoanHeat = maxWeaponHeat;
            weaponOverheated = true;

            //the same thing as before, but if we are destroyed, we don´t lose reference objects.
            UIController.me.overheatedText.gameObject.SetActive(true);
        }

        allGameGuns[currentGunWearing].muzzleFlash.SetActive(true);
       
        //tell everyone to see the shoot sound and muzzle effect in their game
        photonView.RPC("ShootingSound", RpcTarget.All);
    }

    //this is the way to tell all or especific players conected in that room, that something must happen in this server, player or room.
    [PunRPC]
    //this is the function that will be called to a player when he took damage 
    public void DealDamage(string damager, int damageAmount, int actor)
    {
        //if it´s my controller, do this function
        if(photonView.IsMine)
        {
           currentHealth -= damageAmount; //lose health
           UIController.me.healthSlider.value = currentHealth; //update health bar

           if(currentHealth <= 0)
           {
              currentHealth = 0;
              UIController.me.healthSlider.value = currentHealth;
              PlayerSpawner.instance.Die(damager);

              //Call the event and tell the that the damager player to increase one of its kills
              MatchManager.instance.UpdateCharacterStatusSend(actor, 0, 1);
           }
        }
        
    }
   


    void switchGun()
    {
        //for every element present in the array, go acessing one by one, desactivating one by one, until finish the list

        //in foreach, -> 1: you have to put the type of variable you are creating
                      // 2: you put a name to the new variable
                      // 3: you type "in"
                      // 4: type the array or list you are gonna throught, element by element.

        foreach (Gun gun in allGameGuns)
        {
            gun.gameObject.SetActive(false);
        }

        allGameGuns[currentGunWearing].gameObject.SetActive(true);

        allGameGuns[currentGunWearing].muzzleFlash.SetActive(false);
    }

    //this is to update in all game server that we change our current weapon, so everyone will see we holding our weapon changed
    [PunRPC]
    public void ChangeMyGunToEveryoneConnected(int currentWeaponNumber)
    {
        //to avoid conflicts of different numbers in games
        if(currentWeaponNumber < allGameGuns.Length)
        {
            //this will store the value of the were passed from other player so we can update it in everyone else games
            currentGunWearing = currentWeaponNumber;
            switchGun();
        }
    }

    //had to create this delay because if you hide the model at the begin, we will have animations problems
    IEnumerator DelayToHideThisModel()
    {
        yield return new WaitForSeconds(0.3f);
        //hide our player model in our game, but appears to everyone in their game
        playerModelToHide.SetActive(false);
    }

    [PunRPC]     
    public void ShootingSound()
    {
        allGameGuns[currentGunWearing].shotSound.Stop();
        allGameGuns[currentGunWearing].shotSound.Play();

        allGameGuns[currentGunWearing].muzzleFlash.SetActive(true);

        StartCoroutine(Muzzle());
    }
    

       
    //show the muzzle effect of the gun to everyone
     IEnumerator Muzzle()
    {
        yield return new WaitForSeconds(0.1f);

        allGameGuns[currentGunWearing].muzzleFlash.SetActive(false);

    }


    
}


