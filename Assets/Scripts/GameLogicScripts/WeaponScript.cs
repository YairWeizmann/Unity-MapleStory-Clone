using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour
{
    [Header("Weapon Name")]
    public string WeaponName;

    [Header ("Weapon Stats")]
    public float WeaponDamage;

    public float WeaponAttackSpeed;

    public Vector2 WeaopenAttackDistance;

    public float WeaponKnockBack;

    [Header("Weapon Id")]
    public int ItemId;

    [Header ("Weapon Particles")]
    public GameObject WeaopenVfx;

}


