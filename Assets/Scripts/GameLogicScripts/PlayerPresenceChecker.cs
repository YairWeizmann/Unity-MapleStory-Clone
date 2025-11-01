using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPresenceChecker : MonoBehaviour
{
    public List<GameObject> PlayersInScene = new List<GameObject>();


    public void Awake()
    {
        if(PlayersInScene.Count <= 0)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayersInScene.Add(collision.gameObject);
            if(PlayersInScene.Count > 0)
            {
                transform.GetChild(0).gameObject.SetActive(true);
            }
        }

    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayersInScene.Remove(collision.gameObject);
            if(PlayersInScene.Count <= 0)
            {
                transform.GetChild(0).gameObject.SetActive(!gameObject.activeSelf);
            }
                
        }
    }
}
