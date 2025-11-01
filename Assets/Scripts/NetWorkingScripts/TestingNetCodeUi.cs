using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class TestingNetCodeUi : MonoBehaviour
{
    [SerializeField] Button HostBtn;
    [SerializeField] Button ClientBtn;


    private async void Awake()
    {
        try
        {
            // Initialize Unity Services
            await UnityServices.InitializeAsync();

            // Sign in anonymously (if using Authentication)
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in anonymously.");
            }

            Debug.Log("Unity Services initialized successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
        }


        HostBtn.onClick.AddListener(() =>
        {
            Debug.Log("Started Host");
            RelayManagerScript.instance.CreateRelay();
            Hide();
        });


        ClientBtn.onClick.AddListener(() =>
        {
            Debug.Log("Started Client");
            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
