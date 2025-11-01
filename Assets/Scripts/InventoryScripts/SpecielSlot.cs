using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpecielSlot : InventorySlot , IDropHandler
{
    public ItemType acceptedItemType; // The type of item this slot accepts


    public void OnDrop(PointerEventData eventData)
    {

       

        InventoryItem inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
        if (inventoryItem != null)
        {
            if (inventoryItem.Item.type == acceptedItemType)
            {
                base.OnDrop(eventData);

                inventoryManagerScript.EquipItem(inventoryItem.Item);
            }
            else
            {

            }
        }
            
       
    }

}
