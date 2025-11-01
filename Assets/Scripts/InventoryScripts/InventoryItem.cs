    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
  using UnityEngine.UI;
 using UnityEngine.EventSystems;
using Unity.Netcode;
using TMPro;

public class InventoryItem : NetworkBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
    [HideInInspector] public Item Item;

    [Header("UI")]
    public Image ItemImage;
    public TextMeshProUGUI ItemQuantityText;

    [HideInInspector] public int ItemQuantity = 1;

    [HideInInspector] public Transform parentAfterDrag;
    private Canvas canvas;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();


      

        if (Item != null)
        {
            IntializeItem(Item);
        }
    }

    public void IntializeItem(Item newItem)
    {
        if (newItem == null)
        {
            Debug.LogError("Attempted to initialize InventoryItem with a null Item.");
            return;
        }

        Item = newItem;
        ItemImage.sprite = newItem.Image;
        IntiailizeText();
    }

    public void IntiailizeText()
    {
        if (Item == null)
        {
            Debug.LogError("Attempted to initialize text with a null Item.");
            return;
        }

        ItemQuantityText.text = ItemQuantity.ToString();

        bool setActiveText = ItemQuantity > 1;
        ItemQuantityText.gameObject.SetActive(setActiveText);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
       UnParent();

    }

    public void OnDrag(PointerEventData eventData)
    {
        FollowMouse();
    }


    public void OnEndDrag(PointerEventData eventData)
    {

        SetAsParent();
    }

   
    public void SetAsParent()
    {
        ItemImage.raycastTarget = true;

        transform.SetParent(parentAfterDrag, false);
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
    }

    public void FollowMouse()
    {
        transform.position = Input.mousePosition;

    }

    public void UnParent()
    {
        ItemImage.raycastTarget = false;
        parentAfterDrag = transform.parent;

        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();
        transform.localScale = Vector3.one;
    }
}
