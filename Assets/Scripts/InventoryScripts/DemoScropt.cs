using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DemoScropt : MonoBehaviour
{
    public InventoryManager inventoryManager;

    public Item[] itemsToPickUp;

    public void PickUpItem(int id)
    {
        inventoryManager.AddItem(itemsToPickUp[id]);
    }


    public void GetSelectedItem()
    {
       
    }

    public void UseSelectedItem()
    {
     
    }
}
