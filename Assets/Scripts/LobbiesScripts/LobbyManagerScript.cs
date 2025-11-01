using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
using QFSW.QC;
using Unity.IO.LowLevel.Unsafe;
using Unity.Mathematics;

public class LobbyManagerScript : NetworkBehaviour
{
    
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float LobbyUpdateTimer;
    private string playerName = "Yair";
    public static LobbyManagerScript instance;

    private void Awake()
    {
        instance = this;
    }
    private async void Start()
    {
       await UnityServices.InitializeAsync();
       



        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed In! " + AuthenticationService.Instance.PlayerId);
        };

       await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }


    public void Update()
    {
        HandleLobbyHeartBeat();
        PollingLobbyUpdate();
    }

    private async void HandleLobbyHeartBeat()
    {
        if(hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if(heartbeatTimer < 0)
            {
                float heartbeatTimerHolder = 15;
                heartbeatTimer = heartbeatTimerHolder;


               await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void PollingLobbyUpdate()
    {
        if (joinedLobby != null)
        {
            LobbyUpdateTimer -= Time.deltaTime;
            if (LobbyUpdateTimer <= 0)
            {
                float lobbyUpdateHolder = 1.25f;
                LobbyUpdateTimer = lobbyUpdateHolder;

               Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
               joinedLobby = lobby;

            }
        }
    }


    [Command]
    public async void CreateLobby()
    {
        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = true,
                Player = GetPlayer()
                ,
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode" , new DataObject(DataObject.VisibilityOptions.Public,"CO-OP")}
                }
            };

            string lobbyName = "MyLobby";
            int maxPlayers = 4;
            Lobby Created_Lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers,createLobbyOptions);
            Debug.Log("CreatedLobby "+ Created_Lobby.Name + " Max Players: " + Created_Lobby.MaxPlayers+ "Lobby Id:" + Created_Lobby.Id +" " + Created_Lobby.LobbyCode);
            

            hostLobby = Created_Lobby;

            joinedLobby = hostLobby;

            PrintPlayers(joinedLobby);

        }
        catch (LobbyServiceException ex)
        {
           Debug.Log(ex);
        }
    }

    [Command]
    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter> { new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT) },
                Count = 25

            };

            QueryResponse LobbiesResponse = await Lobbies.Instance.QueryLobbiesAsync(); // Gives me all the lobbies That Has Been Created;


            foreach (Lobby lobby in LobbiesResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers +" " + lobby.Data["GameMode"].Value);
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }
    [Command]    
    
    private async void JoinByCodeLobby(string LobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinlobbyByCodeOpt = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

         Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(LobbyCode,joinlobbyByCodeOpt);
            joinedLobby = lobby;

            Debug.Log("Joined Lobby With Code: " + LobbyCode);
            PrintPlayers(joinedLobby);
        }
        catch(LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

    [Command]   
    private async void QuickJoinlobby()
    {
        try
        {
            await Lobbies.Instance.QuickJoinLobbyAsync();
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
            
        }
    }
    [Command]
    private void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }

    [Command]
    public void PrintPlayers(Lobby lobby)
    {
        try
        {
            if (lobby != null)
            {
                Debug.Log("Players In Lobby " + lobby.Name + " " + lobby.Data["GameMode"].Value);
                foreach (Player player in lobby.Players)
                {
                    Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
                }
            }
                
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
       
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {

            { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
        };
    }

    [Command]
    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
           hostLobby =  await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
    {
        { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
    }
            });
            joinedLobby = hostLobby;
            PrintPlayers(hostLobby);
        }

        
        catch (LobbyServiceException ex)
        {
             Debug.Log(ex);
        }
  
    }
    [Command]
    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
           await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {

            { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
            });
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
       
    }


    [Command]

    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            hostLobby = null;
            joinedLobby = hostLobby;
            Debug.Log($"RemovedPlayer {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }
    [Command]
    private async void DisbandLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
            Debug.Log("Lobby disbanded.");
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to disband lobby: {ex.Message}");
        }
    }

    private async void MigrateLobbyData()
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id
            });

            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }

}
