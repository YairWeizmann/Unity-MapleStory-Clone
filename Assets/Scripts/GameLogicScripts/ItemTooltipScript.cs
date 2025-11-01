using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
public class ItemTooltipScript : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler
{
   private InventoryItem InventoryItemScript;
   private ItemToolTipStats itemTooltipScript;




    private void Awake()
    {
       PlayerLogic player = transform.root.GetComponent<PlayerLogic>();
        if (player == null)
        {
            Debug.LogError("PlayerLogic is null on InventoryItem.");
            return;
        }

        itemTooltipScript = player.GetComponentInChildren<ItemToolTipStats>();
        if (itemTooltipScript == null)
        {
            Debug.LogError("ItemToolTipStats component is missing in player's children.");
        }

        InventoryItemScript = GetComponent<InventoryItem>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Hovering");
        ShowToolTip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideToolTip();
    }


    public void ShowToolTip()
    {
        SetTooltipVisibility(true);
        SetItemToToolTip(itemTooltipScript);
    }

    public void HideToolTip()
    {
        SetTooltipVisibility(false);
    }

    private void SetTooltipVisibility(bool isVisible)
    {
        if (itemTooltipScript != null)
        {
            itemTooltipScript.transform.GetChild(0).gameObject.SetActive(isVisible);
        }
        else
        {
            Debug.LogError("ItemToolTipStats component not found when trying to set tooltip visibility.");
        }
    }

    public void SetItemToToolTip(ItemToolTipStats itemToolTipStatsScript)
    {
        if (InventoryItemScript != null)
        {
            itemToolTipStatsScript.ItemIcon.sprite = InventoryItemScript.Item.Image;
            itemToolTipStatsScript.itemTitle.text = InventoryItemScript.Item.name;
            itemTooltipScript.itemDesc.text = InventoryItemScript.Item.itemDesc;
            itemToolTipStatsScript.itemStats.text = "Attack: " + InventoryItemScript.Item.itemDamage + "\nKnockBack: " + InventoryItemScript.Item.itemKnockBack + "\nSpeed: " + InventoryItemScript.Item.itemSpeed +"\nItem Prot: " + InventoryItemScript.Item.itemProt;
        }
    }
}
