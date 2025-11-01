using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShopManager : NetworkBehaviour
{
    [Header("UI Related")]
    [SerializeField] public List<GameObject> ItemsBars = new List<GameObject>();
    private List<GameObject> NpcsItems = new List<GameObject>();

    [Header("Icons")]
    public Image NpcIcon;

    [Header("Scripts")]
    private InventoryManager PlayerInventoryManagerScript;
    private NpcShopOwner NpcShopOwner;


    public void Start()
    {
        GameObject PlayerGm = transform.root.gameObject;
        PlayerInventoryManagerScript = PlayerGm.GetComponentInChildren<InventoryManager>();
    }

    public void SetNpcShopOwner(NpcShopOwner npcShopOwner)
    {
        NpcShopOwner = npcShopOwner;

    }

    public void SetItems(List<GameObject> npcItems)
    {
        if (IsOwner)
        {
            if (npcItems.Count > ItemsBars.Count)
            {
                Debug.LogWarning("Not enough item bars to display all items.");
                return;
            }

            SetButtonItem();
            // Loop through each item bar and assign the corresponding NPC item image
            for (int i = 0; i < npcItems.Count; i++)
            {
                GameObject itemBar = ItemsBars[i]; // Get the current item bar

                // Find the Image component in the item bar
                Image itemImage = itemBar.transform.Find("ItemImage").GetComponent<Image>();


                TextMeshProUGUI ItemsText = itemBar.GetComponentInChildren<TextMeshProUGUI>();

                // Get the NPC item script and item
                ItemScript NpcItemScript = npcItems[i].gameObject.GetComponent<ItemScript>();
                Item NpcItem = NpcItemScript.item;


                // Set image
                if (itemImage != null && NpcItem.Image != null)
                {
                    itemImage.sprite = NpcItem.Image;
                    itemImage.SetNativeSize();

                    RectTransform ItemRect = itemImage.GetComponent<RectTransform>();

                    ItemRect.sizeDelta += new Vector2(25, 27);
                }

                // Set text
                if (ItemsText != null)
                {
                    WeaponScript CurrentWeapon = NpcItemScript.gameObject.GetComponent<WeaponScript>();

                    if (CurrentWeapon != null)
                        ItemsText.text = CurrentWeapon.WeaponName;
                }

                SetNpcIcon();

            }
        }
       
    }


    public void SetNpcIcon()
    {
        NpcIcon.sprite = NpcShopOwner.ShopOwnerImage.sprite;
        NpcIcon.SetNativeSize();
    }

    public void SetButtonItem()
    {
        int maxIndex = ItemsBars.Count;

        for (int i = 0; i < maxIndex; i++)
        {
            GameObject itemBar = ItemsBars[i];  // Get the current item bar (UI element)
            Button buyButton = itemBar.GetComponentInChildren<Button>();  

            if (buyButton != null)
            {
                GameObject npcItem = NpcsItems[i];  
                buyButton.onClick.RemoveAllListeners();  
                buyButton.onClick.AddListener(() => OnBuyButton(npcItem)); 
            }
            else
            {
                Debug.LogWarning("No Button found in the item bar: " + itemBar.name);
            }
        }
    }

    public void OnBuyButton(GameObject npcItem)
    {

        GameObject PlayerObject = transform.root.gameObject; // Get The Player GAmeObject

        PlayerInventoryManagerScript = PlayerObject.GetComponentInChildren<InventoryManager>(); // Search For His InventoryManager Script In His Children

        Item itemToBuy = npcItem.GetComponent<ItemScript>().item;

        PlayerInventoryManagerScript.AddItemServerRpc(itemToBuy.ItemId, itemToBuy.type);

    }

    public void OnExitButton()
    {

        Transform CanvasPannel = this.transform.GetChild(0);
        CanvasPannel.gameObject.SetActive(false);
        
    }

    public void OnSwordsButton()
    {
        if(NpcShopOwner != null)
        {
            NpcsItems = NpcShopOwner.NpcItemsSwords;
            
            SetItems(NpcsItems);

            SetButtonItem();
        }
    }

    public void OnArmorButton()
    {
        NpcsItems = NpcShopOwner.NpcItemsArmor;
        SetItems(NpcsItems);
        SetButtonItem();


    }

    public void OnItemsButton()
    {
        NpcsItems = NpcShopOwner.NpcItemsMobDrops;
        SetItems(NpcsItems);
        SetButtonItem();


    }


}
