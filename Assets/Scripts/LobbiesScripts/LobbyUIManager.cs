using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] private Button CreateGameButton;
    [SerializeField] private GameObject PannelCanvas;
    [SerializeField] private GameObject InputWindowPannel;

    private void Start()
    {
    }


    public void OnClickCreateGameButton()
    {
        RelayManagerScript.instance.CreateRelay();
        SceneLoaderManager.instance.LoadAllScenes();
        CreateGameButton.enabled = false;
        PannelCanvas.SetActive(false);

    }

    public void OnJoinGameButton()
    {
        InputWindowPannel.SetActive(!InputWindowPannel.activeSelf);
    }

    public void OnJoinGameOkButton()
    {
        TMP_InputField input_Field = InputWindowPannel.GetComponentInChildren<TMP_InputField>();
        string JoinedCode = input_Field.text;

        RelayManagerScript.instance.JoinRelay(JoinedCode);
        InputWindowPannel.SetActive(false);
        PannelCanvas.SetActive(false);
    }
    public void OnCancelJoinGameButton()
    {
        InputWindowPannel.SetActive(false);
    }
}
