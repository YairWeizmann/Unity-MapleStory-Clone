using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header ("Speed Handler")]
    public float currentCrawlSpeed;
    public float currentSpeed;

    [Header("Damage Handler")]
    public float currentDamage;
    private float maxDamage;


    [Header("Particles Handler")]
    public GameObject DashUpParticle;
    public GameObject DashForwardParticle;

    [Header("Sword Slash Particles")]
    public GameObject CurrentSlashParticle;
}
