using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManagerScript : MonoBehaviour
{
    public static RelayManagerScript instance;
    public void Awake()
    {
        instance = this;
    }

    [Command]
   public async void CreateRelay()
    {
        try
        {
          Allocation allocation =  await RelayService.Instance.CreateAllocationAsync(5);

          string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(joinCode);


            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);
            NetworkManager.Singleton.StartHost();
        }
        catch(RelayServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    [Command]
    public async void JoinRelay(string joinCode)
    {
        try
        {
           JoinAllocation joinAll =  await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(joinAll.RelayServer.IpV4, (ushort)joinAll.RelayServer.Port, joinAll.AllocationIdBytes, joinAll.Key, joinAll.ConnectionData,joinAll.HostConnectionData);


            NetworkManager.Singleton.StartClient();

        }
        catch(RelayServiceException ex)
        {
            Debug.Log(ex);
        }
    }

}
