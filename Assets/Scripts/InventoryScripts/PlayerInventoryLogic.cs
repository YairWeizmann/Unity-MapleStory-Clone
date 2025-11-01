using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static QuestsManager;

public class PlayerInventoryLogic : NetworkBehaviour
{
    [SerializeField] private GameObject Inventory;
    [SerializeField] private GameObject ToolBar;


  public InventoryManager inventoryManagerScript;

    public void Start()
    {
        inventoryManagerScript = ToolBar.GetComponentInChildren<InventoryManager>();
    }

    public void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(KeyCode.I))
            ToggleInventory();

        bool isKeyPressed = Input.GetKeyDown(KeyCode.U);

        if (isKeyPressed )
        {

            if (inventoryManagerScript != null)
            {
                inventoryManagerScript.GetSelectedItem(true , isKeyPressed);
            }

            else
                Debug.Log("No InventoryManager");
        }
            

    }


    //Inventory Logic 

    #region InventoryLogic
    public void ToggleInventory() // Toggling Main Inventory
    {
        Inventory.SetActive(!Inventory.activeSelf);
    }

    public void OnTriggerEnter2D(Collider2D collision) //Collecting Drops
    {
        bool ShouldCollect = collision.gameObject.CompareTag("MobDrop");

        if (ShouldCollect)
        {
            ItemScript DroppedItem = collision.gameObject.GetComponent<ItemScript>();


            if (DroppedItem.item != null)
            {
                InventoryManager InventoryMangerScript = ToolBar.GetComponentInChildren<InventoryManager>();


                if (InventoryMangerScript != null)
                {
                    CheckForActivePickingQuest(DroppedItem.item);
                    InventoryMangerScript.AddItemServerRpc(DroppedItem.item.ItemId,DroppedItem.item.type);
                }
            }

            NetworkObject DroppedNet = DroppedItem.GetComponent<NetworkObject>();

            if (IsOwner && DroppedNet != null) // Ensure only the client who collected despawns
            {
                DespawnMobDropServerRpc(DroppedNet.NetworkObjectId);
            }
            if (DroppedNet == null)
                DespawnLocally(DroppedItem.gameObject);

        }
    }

    #endregion

    //ServerMethods

    #region Server Methods

    [ServerRpc(RequireOwnership = false)]
    public void DespawnMobDropServerRpc(ulong NetobjiD)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(NetobjiD, out var networkObject))
        {
            networkObject.Despawn(true);
        }
    }

    public void DespawnLocally(GameObject MobDrop)
    {
        Destroy(MobDrop);
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            Inventory.SetActive(false);
            ToolBar.SetActive(true); // Ensure ToolBar is active for the owner
        }
        else
        {
            ToolBar.SetActive(false); // Ensure ToolBar is not active for non-owners
        }
    }
    #endregion


    public void CheckForActivePickingQuest(Item item)
    {
        PlayerLogic PlayerLogicScript = GetComponent<PlayerLogic>();
        if (PlayerLogicScript.activeQuests.Any(quest => quest is PickUpQuest))
        {

            foreach (var quest in PlayerLogicScript.activeQuests)
            {
                if (quest is PickUpQuest pickUpQuest && pickUpQuest.ItemToCollect == item)
                {
                    pickUpQuest.PickedUpItem(true);
                    break;
                }
            }

        }
    }

}

public enum ActionTypes
{
    UseItem,
    ThrowItem,
    BuyItem,
    SellItem
}



