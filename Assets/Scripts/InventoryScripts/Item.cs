using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu (menuName = "ItemDataSO")]
public class Item : ScriptableObject
{
    [Header("Only GamePlay")]  
    public ItemType type;
    public ActionType actionType;
    public Vector2Int range = new Vector2Int(5,4);

    [Header("Item Abilitis")]
    public string itemName;
    public string itemDesc;

    [Header("Item Stats")]
    public int itemDamage;
    public float itemKnockBack;
    public int itemSpeed;
    public int itemProt;

    [Header("Only UI")]
    public bool StackAble = false;

    [Header("Both")]
    public Sprite Image;

    [Header("Idetfiers")]
    public int ItemId;

}


public enum ItemType
{
    Weapon,
    Armor,
    Helmet,
    Potion,
    MobDrop,
    Skill
}

public enum ActionType
{
    Attack,     // For Sword
    Equip,
    Consume,    // For Potion
    Collect,    // For MobDrop
    Sell,       // For MobDrop
    Use,       // For Skill
}
