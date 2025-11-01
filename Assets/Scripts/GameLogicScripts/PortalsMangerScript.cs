using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public enum PortalLocation
{
    PinkLadyPortal,
    BigCityPortal,
    CastleGate,
    DungeonEntrance,
    // Add other portal locations as needed
}
public class PortalsMangerScript : NetworkBehaviour
{
    public static PortalsMangerScript instance;
    public PortalLocation currentPortal;  // Enum representing this portal's location
    public PortalLocation targetPortal;
    public void Awake()
    {
        instance = this;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        NetworkObject netobj = collision.GetComponent<NetworkObject>();
        if (netobj == null)
        {
            Debug.LogError("NetObj is Null");
            return;
        }

        if (netobj.IsOwner && collision.CompareTag("Player"))
        {

            PlayerLogic playerLogicScript = netobj.GetComponent<PlayerLogic>();
            if (playerLogicScript != null)
            {
                playerLogicScript.ShouldEnterPortal = true;
                SceneLoaderManager.instance.GotIntoPortal(gameObject, netobj);
            }


        }
    }

    public Transform GetTargetPortal()
    {
        // Convert targetPortal enum to string to find the corresponding GameObject
        string targetPortalName = targetPortal.ToString();

        GameObject targetPortalObject = GameObject.Find(targetPortalName);
        if (targetPortalObject != null)
        {
            return targetPortalObject.transform;
        }

        Debug.LogError($"Target portal '{targetPortalName}' not found!");
        return null;
    }


}
