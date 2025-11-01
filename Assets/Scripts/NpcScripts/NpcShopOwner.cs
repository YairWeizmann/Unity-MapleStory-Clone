using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NpcShopOwner : NetworkBehaviour
{
   private NpcSaying PlayerInRangeScript;

  [Header("Shop Owner Items")]
  [SerializeField] public List<GameObject> NpcItemsArmor = new List<GameObject>();

  [SerializeField] public List<GameObject> NpcItemsSwords = new List<GameObject>();

  [SerializeField] public List<GameObject> NpcItemsMobDrops = new List<GameObject>();

    [Header("Shop Icons")]
    public Image ShopOwnerImage;

  [Header("GameLogic")]
  [HideInInspector]  public List<GameObject> PlayersInRange = new List<GameObject>();

    public void OnShopButtonClick()
    {

       
            foreach (GameObject player in PlayersInRange)
            {
                PlayerShopManager shopCanvasTransform = player.GetComponentInChildren<PlayerShopManager>();



                NpcDialouge playerCanvas = player.GetComponentInChildren<NpcDialouge>();

                if (shopCanvasTransform != null)
                {
                    
                    shopCanvasTransform.transform.GetChild(0).gameObject.SetActive(true);

                    

                    shopCanvasTransform.SetNpcShopOwner(this);


                    // shopCanvasTransform.SetItems(NpcItemsSwords);

                    //  shopCanvasTransform.SetButtonItem();
                }

            }
       
        

    }




    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

            if (!PlayersInRange.Contains(collision.gameObject))
            {
                // Add the player to the list
                PlayersInRange.Add(collision.gameObject);
                Debug.Log(collision.gameObject.name);


            }

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Check if the player is in the list
            if (PlayersInRange.Contains(collision.gameObject))
            {
                // Remove the player from the list
                PlayersInRange.Remove(collision.gameObject);
            }
        }
    }
}
