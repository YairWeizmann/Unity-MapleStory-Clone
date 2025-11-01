using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseHoverText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI questText;  // Assign your TextMeshProUGUI component in the Inspector
    private float originalSize;
    private Vector3 GameObjectOriginialSize;
    private void Start()
    {
        if (questText == null)
        {
            questText = GetComponent<TextMeshProUGUI>();
        }
        originalSize = questText.fontSize;
        GameObjectOriginialSize = this.gameObject.transform.localScale;
    }

    // When the mouse hovers over the text
    public void OnPointerEnter(PointerEventData eventData)
    {
        questText.fontSize = originalSize * 1.005f;  // Increase size by 20%
        this.gameObject.transform.localScale  = new Vector3(1.25f,1.25f,1f);
    }

    // When the mouse stops hovering
    public void OnPointerExit(PointerEventData eventData)
    {
        questText.fontSize = originalSize;  // Return to original size
        this.gameObject.transform.localScale = GameObjectOriginialSize;

    }

}
