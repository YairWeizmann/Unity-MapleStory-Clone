using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum skillType
{
    HolySpirit,
    LightningThunder,
    ShadowNight
}

public class SkillBase : MonoBehaviour
{
    protected string SkillName;

    public skillType skillType;

    public float ManaCost;

    public float CoolDown;

    protected float currentCoolDown;

    protected bool IsReady;



    
}


