using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NpcDialouge : NetworkBehaviour
{
    [Header("Npc Related")]
    public GameObject NpcObj;
    public Image NpcIcon;
    public TextMeshProUGUI npcNameTxt;

    [Header("Dialouge Related")]
    public TextMeshProUGUI SentencesText;

    public float TextSpeed;

    private List<string> WhatsTheNpcSays = new List<string>();
    private int WhatsTheNpcSayCount = 0;
    private bool isTyping = false;


    public void DisplayDialouge(Image NpcImage, List<string> NpcSays, string NpcName , GameObject Npc)
    {
        if (IsOwner)
        {
            NpcObj = Npc;
            Transform child = this.gameObject.transform.GetChild(0);
            child.gameObject.SetActive(true);

            NpcIcon.sprite = NpcImage.sprite;

            NpcIcon.SetNativeSize();

            npcNameTxt.text = NpcName;
            WhatsTheNpcSays = NpcSays;
            WhatsTheNpcSayCount = 0; // Reset counter when new dialogue starts
            SentencesText.text = "";


            StopAllCoroutines(); // Stoping All Couroutines before assigning the Text

            StartCoroutine(TypeSentence(WhatsTheNpcSays[WhatsTheNpcSayCount])); // Start the coroutine for the typing effect
        }
        else
            return;
       
    }

    public void OnNextButtonClick()
    {
        if (isTyping)
        {
            StopAllCoroutines(); // Stop the current typing effect
            SentencesText.text = WhatsTheNpcSays[WhatsTheNpcSayCount]; // Show the complete sentence immediately
            isTyping = false;
            return;
        }

        WhatsTheNpcSayCount++;

        if (WhatsTheNpcSayCount < WhatsTheNpcSays.Count)
        {
            Debug.Log(WhatsTheNpcSayCount);

            StartCoroutine(TypeSentence(WhatsTheNpcSays[WhatsTheNpcSayCount])); // Display next sentence
        }
        else
        {
            OnExitButton();
            WhatsTheNpcSayCount = 0;
            return;
        }
    }

    public void OnPrevButtonClick()
    {
        if (isTyping)
        {
            StopAllCoroutines(); // Stop the current typing effect
            SentencesText.text = WhatsTheNpcSays[WhatsTheNpcSayCount]; // Show the complete sentence immediately
            isTyping = false;
            return;
        }

        WhatsTheNpcSayCount--;

        if (WhatsTheNpcSayCount >= 0)
        {
            Debug.Log(WhatsTheNpcSayCount);

            StartCoroutine(TypeSentence(WhatsTheNpcSays[WhatsTheNpcSayCount])); // Display previous sentence
        }
        else
        {
            Debug.Log("Start of dialogue");
            WhatsTheNpcSayCount = 0;
            return;
        }
    }

    public void OnExitButton()
    {
        SentencesText.text = "";
        npcNameTxt.text = "";
        NpcIcon.sprite = null;

        Transform child = this.gameObject.transform.GetChild(0);
        child.gameObject.SetActive(false);

    }

    public IEnumerator TypeSentence(string sentence)
    {
        SentencesText.text = ""; // Clear the text before starting
        isTyping = true;

        foreach (char letter in sentence)
        {
            SentencesText.text += letter; // Add each letter one by one
            yield return new WaitForSeconds(TextSpeed); // Wait for the defined delay
        }

        isTyping = false;
    }

    
}


