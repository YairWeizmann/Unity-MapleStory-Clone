using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class InventoryManager : NetworkBehaviour
{
    [Header("Inventory Related")]
    public InventorySlot[] InventorySlots;
    public GameObject InventoryItemPrefab;
    public int maxStack;
    public int SelectedSlot = 0;
    private Item currentWeapon;


    [Header("Singleton")]
    private static InventoryManager instance;

    [Header("Game Objects")]
    public GameObject Player; // Reference to the entire player GameObject

    [Header("Scripts")]
    private PlayerAttackingLogic playerAttackingLogic;
    private PlayerLogic playerLogicScript;

    bool ShouldRotateWeapon;

    private GameObject SpawnedPrefabs;

    private GameObject PlayerArm;




    #region Start Of The Game


    private void Start()
    {
        if (!IsOwner)
            return;


        NetworkObject networkObject = transform.root.GetComponent<NetworkObject>();
       

        for (int i = 0; i < InventorySlots.Length; i++)
        {
            InventorySlots[i].SlotIndex = i; // Assign the slot index
        }

        ChangeSelectedSlot(SelectedSlot); // Setting The First Selected Slot

        GetComponenetsServerRpc();

      
    }
    private void OnConnectedToServer()
    {
        if (!IsOwner) return;

        Debug.Log("Connected To Server !");

        GetComponenetsServerRpc();

    }

 

    #endregion

    #region Local Related


    [ServerRpc (RequireOwnership = false)]
    public void GetComponenetsServerRpc()
    {
        EquipingManager equipingManagerScript = GetComponent<EquipingManager>();
        playerAttackingLogic = GetComponentInParent<PlayerAttackingLogic>();
        playerLogicScript = GetComponentInParent<PlayerLogic>();

        AccessArmLocally();




    }
    #endregion



    #region Inventory Related

    public void AccessArmLocally()
    {
        playerLogicScript = transform.root.gameObject.GetComponent<PlayerLogic>();

        if (playerLogicScript == null)
            return;

        NetworkObjectReference armReference = playerLogicScript.playerArmReferenceNetworkVar.Value;

       
        
        if (armReference.TryGet(out NetworkObject playerArmNet))
        {
            PlayerArm = playerArmNet.gameObject;



        }
    }

    public void ChangeSelectedSlot(int newValue)
    {

        SelectedSlot = newValue;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddItemServerRpc(int itemId, ItemType ItemType)
    {

        
            Item item = GetItemById(itemId, ItemType);
            if (item == null)
            {
                Debug.LogError("item is Null");
                return;
            }
            else if(item != null)
            {
                AddItemClientRpc(item.ItemId, item.type);

            }
            
        
        
       
    }

    [ClientRpc]
    private void AddItemClientRpc(int itemId ,ItemType ItemType)
    {
        Item item = GetItemById(itemId, ItemType);
        if (item != null)
        {
            AddItem(item);
        }
    }

    public bool AddItem(Item item)
    {
        for (int i = 0; i < InventorySlots.Length; i++)
        {
            InventorySlot slot = InventorySlots[i];

            InventoryItem ItemInSlot = slot.GetComponentInChildren<InventoryItem>();

            bool ShouldStack = ItemInSlot != null && ItemInSlot.Item.ItemId == item.ItemId && ItemInSlot.ItemQuantity < maxStack && ItemInSlot.Item.StackAble;

            if (ShouldStack)
            {
                ItemInSlot.ItemQuantity++;
                ItemInSlot.IntiailizeText();
                return true;
            }
        }

        for (int i = 0; i < InventorySlots.Length; i++)
        {
            InventorySlot slot = InventorySlots[i];

            InventoryItem ItemInSlot = slot.GetComponentInChildren<InventoryItem>();

            if (ItemInSlot == null)
            {
                SpawnItem(item, slot);
                return true;
            }
        }

        return false;
    }

    private void SpawnItem(Item item, InventorySlot slot)
    {
        GameObject newItemGo = Instantiate(InventoryItemPrefab, slot.transform);

        InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
        inventoryItem.IntializeItem(item);
    }

    public void GetSelectedItem(bool use,bool KeyPressed)
    {
        EquipingManager equipingManagerScript = GetComponent<EquipingManager>();

        InventorySlot slot = InventorySlots[SelectedSlot];
        if (slot == null)
        {
            Debug.LogError("Selected slot is null.");
            return; // Exit the method if the slot is null
        }

        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
        if (itemInSlot != null)
        {
            // Determine if the item is equipable based on the new lists
            bool isEquipable = false;

            // Check the type of item to see if it's in the respective list
            if (itemInSlot.Item.type == ItemType.Armor && itemInSlot.Item.ItemId < equipingManagerScript.Armor.Count)
            {
                isEquipable = true;
            }
            else if (itemInSlot.Item.type == ItemType.Weapon && itemInSlot.Item.ItemId < equipingManagerScript.Swords.Count)
            {
                isEquipable = true;
            }
            else if (itemInSlot.Item.type == ItemType.Skill && itemInSlot.Item.ItemId < equipingManagerScript.Skills.Count)
            {
                isEquipable = true;
            }
            else if (itemInSlot.Item.type == ItemType.Helmet && itemInSlot.Item.ItemId < equipingManagerScript.Helmet.Count)
            {
                isEquipable = true;
            }
            else if (itemInSlot.Item.type == ItemType.MobDrop && itemInSlot.Item.ItemId < equipingManagerScript.MobDrops.Count)
            {
                isEquipable = false;
            }


            // Handle item usage or equipping
            if (use && !isEquipable)
            {
                if (itemInSlot.Item.StackAble)
                {
                    itemInSlot.ItemQuantity--;

                    if (itemInSlot.ItemQuantity <= 0)
                        Destroy(itemInSlot.gameObject);
                    else
                        itemInSlot.IntiailizeText();
                }
            }
            else if (use && isEquipable && !KeyPressed)
            {
                Debug.Log("Should Equip");
                EquipItem(itemInSlot.Item);
            }
        }

    }

    public void EquipItem(Item Item)
    {


        AccessArmLocally(); // Acsess The Arm Again Since Not Synced


        if (Item.type == ItemType.Armor || Item.type == ItemType.Helmet)
        {
            SpawnOnTheNetServerRpc(Item.ItemId , Item.type);
             

        }
        else if (Item.type == ItemType.Weapon)
        {
            Item GetCurrentItem = Item;
            if (PlayerArm.transform.childCount <= 0)
            {
                if (currentWeapon == null)
                    currentWeapon = Item;

                SpawnOnTheNetServerRpc(Item.ItemId, Item.type);
            }
            else if (currentWeapon.ItemId != Item.ItemId)
            {
                // If the new item is different from the currently equipped weapon
                NetworkObject WeaponNet = PlayerArm.transform.GetChild(0).gameObject.GetComponent<NetworkObject>();
                DestoryAnItemServerRpc(WeaponNet.NetworkObjectId);

                // Update the current weapon to the new item
                currentWeapon = Item;
                SpawnOnTheNetServerRpc(currentWeapon.ItemId,currentWeapon.type);
            }
        }

        else if (Item.type == ItemType.Skill)
        {
           SpawnOnTheNetServerRpc(Item.ItemId,Item.type);

          

        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void DestoryAnItemServerRpc(ulong netObjId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netObjId, out NetworkObject networkObject))
        {
            // Destroy the object on the server

            GameObject gameObject = networkObject.gameObject;
            networkObject.Despawn(true); // The "true" parameter will destroy the GameObject as well
            Destroy(gameObject);
            
        }
        else
        {
            Debug.LogWarning($"NetworkObject with ID {netObjId} not found.");
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void SpawnOnTheNetServerRpc(int itemId , ItemType itemType)
    {

        EquipingManager equipingManagerScript = GetComponent<EquipingManager>();
        GameObject spawnedPrefab = null;

        switch (itemType)
        {
            case ItemType.Armor:
                spawnedPrefab = equipingManagerScript.Armor[itemId]; // Use Armor list
                break;
            case ItemType.Weapon:
                spawnedPrefab = equipingManagerScript.Swords[itemId]; // Use Weapon list
                break;
            case ItemType.Skill:
                spawnedPrefab = equipingManagerScript.Skills[itemId]; // Use Skills list
                break;
            case ItemType.Helmet:
                spawnedPrefab = equipingManagerScript.Helmet[itemId]; // Use Skills list

                break;
            default:
                Debug.LogError("Unknown item type");
                return;
        }
        ItemScript ItemScript = spawnedPrefab.GetComponent<ItemScript>();
        Item SpawnedItem = ItemScript.item;

        if (spawnedPrefab != null)
        {
            GameObject spawnedItem = Instantiate(spawnedPrefab);
            NetworkObject WeaponNet = spawnedItem.GetComponent<NetworkObject>();


            if(SpawnedItem.type == ItemType.Skill)
            {
                PlayerSkills PlayerSkill = GetComponentInParent<PlayerSkills>();
                if (PlayerSkill != null)
                {
                    PlayerSkill.UseSkill(spawnedItem);
                }
            }

            else if (WeaponNet != null) // Means Its Not A Skill
            {
                WeaponNet.Spawn(true);
                Debug.Log("Spawned on The Network");

                if (SpawnedItem.type == ItemType.Armor || SpawnedItem.type == ItemType.Helmet)
                {
                    spawnedItem.transform.SetParent(Player.transform, false);
                    spawnedItem.transform.localPosition = Vector3.zero;
                    SyncItemTransformClientRpc(WeaponNet.NetworkObjectId, Player.GetComponent<NetworkObject>().NetworkObjectId);

                    PlayerGUIManger PlayerGuiManagerScript = transform.GetComponentInParent<PlayerGUIManger>();

                    PlayerMovement PlayerMovementScript = transform.GetComponentInParent<PlayerMovement>();

                    SpriteRenderer SpawnedVisual = spawnedItem.GetComponent<SpriteRenderer>();

                    PlayerGuiManagerScript.EquipArmor(spawnedItem);


                    PlayerMovementScript.PlayerArmor.Add(SpawnedVisual);
                }
                else if (SpawnedItem.type == ItemType.Weapon)
                {
                    spawnedItem.transform.SetParent(PlayerArm.transform, false);
                    SyncItemTransformClientRpc(WeaponNet.NetworkObjectId, PlayerArm.GetComponent<NetworkObject>().NetworkObjectId);

                    if (playerAttackingLogic != null)
                        playerAttackingLogic.IntiliazeWeapon(WeaponNet.gameObject);

                    else
                        Debug.Log("Iceeeeeee berg");
                }

               
            }
        }
    }

    [ClientRpc]
    public void SyncItemTransformClientRpc(ulong itemNetworkId, ulong parentNetworkId)
    {
        NetworkObject itemNet = NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemNetworkId];
        NetworkObject parentNet = NetworkManager.Singleton.SpawnManager.SpawnedObjects[parentNetworkId];

        if (itemNet != null && parentNet != null)
        {
            itemNet.transform.SetParent(parentNet.transform, false);
            itemNet.transform.localPosition = Vector3.zero;
            Debug.Log("Parenting synchronized on client");
        }
    }


    public Item GetItemById(int itemId , ItemType itemType)
    {
       EquipingManager equipingManagerScript = GetComponent<EquipingManager>();

        ItemScript itemScript = null;



        switch (itemType)
        {
            case ItemType.Armor:
                if (itemId < equipingManagerScript.Armor.Count)
                {
                    itemScript = equipingManagerScript.Armor[itemId].GetComponent<ItemScript>();
                }
                break;

            case ItemType.Weapon:
                if (itemId < equipingManagerScript.Swords.Count)
                {
                    itemScript = equipingManagerScript.Swords[itemId].GetComponent<ItemScript>();
                }
                break;

            case ItemType.Skill:
                if (itemId < equipingManagerScript.Skills.Count)
                {
                    itemScript = equipingManagerScript.Skills[itemId].GetComponent<ItemScript>();
                }
                break;
            case ItemType.Helmet:

                if (itemId < equipingManagerScript.Helmet.Count)
                {
                    itemScript = equipingManagerScript.Helmet[itemId].GetComponent<ItemScript>();
                }
                break;

            case ItemType.MobDrop:
                if (itemId < equipingManagerScript.MobDrops.Count)
                {
                    itemScript = equipingManagerScript.MobDrops[itemId].GetComponent<ItemScript>();
                }
                break;

            default:
                Debug.LogError("Unknown item type");
                return null;
        }

        if (itemScript != null)
        {
            return itemScript.item;
        }
        else
        {
            Debug.LogError("Item with ID " + itemId + " not found in type " + itemType);
            return null;
        }


    }
}
#endregion
