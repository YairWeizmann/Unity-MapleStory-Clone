using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
    [HideInInspector] public GameObject PlayerSkill;
     GameObject PlayerObject;

    public void Start()
    {
        PlayerObject = this.gameObject;
    }

    public void UseSkill(GameObject PlayerSkill)
    {
        SkillBase skillBase = PlayerSkill.GetComponent<SkillBase>();


        if (skillBase != null)
        {
            switch (skillBase.skillType)
            {
                case skillType.HolySpirit:
                    Debug.Log("Using" +  PlayerSkill.name);
                    HolySpiritSkill HolySpirtskill = PlayerSkill.GetComponent<HolySpiritSkill>();
                    HolySpirtskill.SkillDoes(PlayerObject);
                    break;
                case skillType.LightningThunder:
                    Debug.Log("Using" + PlayerSkill.name);
                    LightningThunder Lightningskill = PlayerSkill.GetComponent<LightningThunder>();
                    Lightningskill.SkillDoes(PlayerObject);
                    break;
                case skillType.ShadowNight:
                    Debug.Log("Using" + PlayerSkill.name);
                    ShadowNight ShadowNightskill = PlayerSkill.GetComponent<ShadowNight>();
                    ShadowNightskill.SkillDoes(PlayerObject);
                    break;
            }
            
        }
    }
}
