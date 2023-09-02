using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public bool isAutomatic;

    public float timeBetweenShoots = .1f, heatPerShoot = 1f;
    
    public GameObject muzzleFlash;

    public int shotDamage;

    public AudioSource shotSound;
     
}
