using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TypeSentence : MonoBehaviour
{
    [Header("Text To Write")]
   [SerializeField] TextMeshProUGUI SentencesText;

    
   [Header("Text Speed")]
   [SerializeField] public float TextSpeed;

    [Header("Instance")]
    public static TypeSentence Instance;

    public void Awake()
    {
        Instance = this;
    }

    public IEnumerator TypeSentences(string sentence)
    {
        
        SentencesText.text = ""; // Clear the text before starting

        foreach (char letter in sentence)
        {
            SentencesText.text += letter; // Add each letter one by one
            yield return new WaitForSeconds(TextSpeed); // Wait for the defined delay
        }

    }

    public void WriteMessege(string Messege)
    {
        StartCoroutine(TypeSentences(Messege));
    }
    public void CompleteQuest(string questName)
    {
        this.gameObject.SetActive(true);
        StartCoroutine(TypeSentences("Completed Quest: " + questName));
    }


}
