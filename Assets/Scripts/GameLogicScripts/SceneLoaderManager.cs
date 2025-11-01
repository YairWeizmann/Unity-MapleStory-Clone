using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEditor;
using UnityEngine.UIElements;

public class SceneLoaderManager : NetworkBehaviour
{
   [HideInInspector] public static SceneLoaderManager instance;
    [Header("Portal Time")]
    [SerializeField] float time_To_GetInPortal;

    private bool hasLoadedScenes = false; // Track if scenes have already been loaded

    private void Awake()
    {
        DontDestroyOnLoad(this.transform.root.gameObject);
        instance = this;

       //LoadAllScenes();
       
    }


  

    public void GotIntoPortal(GameObject portal, NetworkObject player)
    {
        if (player.IsOwner) 
        {
          StartCoroutine(CheckForPortalInputCoroutine(portal,player,time_To_GetInPortal));
          
        }
        else
        {
            Debug.LogError("Player Is Not Owner");
        }
    }

    public void UnloadAllScenes()
    {
        SceneManager.UnloadSceneAsync("PinkLadyMap");
        SceneManager.UnloadSceneAsync("MapleWorld");

    }

    public void LoadAllScenes()
    {
        SceneManager.LoadSceneAsync("PinkLadyMap", LoadSceneMode.Additive); // Fix Later
        SceneManager.LoadSceneAsync("MapleWorld", LoadSceneMode.Additive); // Fix Later
    }

    private IEnumerator CheckForPortalInputCoroutine(GameObject portal, NetworkObject player, float time)
    {
        // Wait until the player presses the "L" key

        PlayerLogic PlayerLogicScript = player.gameObject.GetComponent<PlayerLogic>();

        while (!Input.GetKeyDown(KeyCode.L) || !PlayerLogicScript.ShouldEnterPortal) yield return null;


        PlayerLogicScript.EnableLoadingScreen();

        yield return new WaitForSeconds(time);
       

        PortalsMangerScript portalScript = portal.GetComponent<PortalsMangerScript>(); 

        if (portalScript != null)
        {
            Transform targetPortal = portalScript.GetTargetPortal();
            if (targetPortal != null)
            {
                player.transform.position = targetPortal.position;
                player.transform.rotation = targetPortal.rotation;

                PlayerLogicScript.DisableLoadingScreenAnim();


            }
        }
    }


    public void HandlePlayer(NetworkObject player)
    {

    }



}
