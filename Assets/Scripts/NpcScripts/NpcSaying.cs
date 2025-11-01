using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

public enum NpcQuests
{
    KillFiveEnemies,
}

public class NpcSaying : MonoBehaviour
{
    [Header("Npc Says")]
    public List<string> NpcSays = new List<string>();
    public Image npcImage;
    public string npcName;


    [Header("Visuals Related")]
    [SerializeField] GameObject ArrowGameObject;

    Button dialogueButton;
 [HideInInspector] public  List<GameObject> PlayersInRange = new List<GameObject>();

    


    public void Start()
    {
        dialogueButton = GetComponentInChildren<Button>();

        dialogueButton.onClick.AddListener(OnDialogueBtnClick);

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

            if (!PlayersInRange.Contains(collision.gameObject))
            {
                // Add the player to the list
                PlayersInRange.Add(collision.gameObject);
            }

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (PlayersInRange.Contains(collision.gameObject))
            {
                PlayersInRange.Remove(collision.gameObject);
            }
        }
    }
    public void OnDialogueBtnClick()
    {
        ArrowGameObject.SetActive(false);

        foreach (GameObject player in PlayersInRange)
        {
            NpcDialouge playerCanvas = player.GetComponentInChildren<NpcDialouge>();

            if (playerCanvas != null)
            {
                playerCanvas.DisplayDialouge(npcImage, NpcSays, npcName, this.transform.gameObject);
            }
          
        }

    }

}
