using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler , IPointerClickHandler 
    {
    public Color SelectedColor, NotSelectedColor;
    public Image ThisImage;

   public int SlotIndex = 0;

    private static InventorySlot currentlySelectedSlot;

   [HideInInspector] public InventoryManager inventoryManagerScript;

    public void Start()
    {
        ThisImage = GetComponent<Image>();

        GameObject PlayerGm = transform.root.gameObject;

        inventoryManagerScript = PlayerGm.GetComponentInChildren<InventoryManager>();
                
        
    }
    public void Selected()
    {
        Color newColor = SelectedColor;

        newColor.a = 1f;

       ThisImage.color = newColor;

        if(inventoryManagerScript != null)
       inventoryManagerScript.ChangeSelectedSlot(SlotIndex);

    }

    public void NotSelected()
    {

        Color newColor = NotSelectedColor;

        newColor.a = 1f;

        ThisImage.color = NotSelectedColor;

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // When the slot is clicked, call the Selected method
        if (currentlySelectedSlot != null && currentlySelectedSlot != this)
        {
            currentlySelectedSlot.NotSelected();
        }

        // Set this slot as the current selected slot
        Selected();
        currentlySelectedSlot = this; // Update the referen
    }

    public void OnDrop(PointerEventData eventData)
    {


        if (transform.childCount == 0)
        {
            InventoryItem inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            if (inventoryItem != null)
            {
                // Update the parentAfterDrag to this slot
                inventoryItem.parentAfterDrag = transform;

                // Optionally: Immediately set the parent (not necessary but can help visualize during drag)
                eventData.pointerDrag.transform.SetParent(transform, false);
                eventData.pointerDrag.transform.localPosition = Vector3.zero; // Align to slot

            }
        }
        else if(transform.childCount == 1)
        {
            InventoryItem draggedItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            

            if (draggedItem != null)
            {
                Transform currentItem = transform.GetChild(0);

                // Swap the items' parents
                currentItem.SetParent(draggedItem.parentAfterDrag, false);
                currentItem.localPosition = Vector3.zero;

                // Set the dragged item's new parent to this slot
                draggedItem.parentAfterDrag = transform;
                draggedItem.transform.SetParent(transform, false);
                draggedItem.transform.localPosition = Vector3.zero;
            }
           
        }
    }


}
