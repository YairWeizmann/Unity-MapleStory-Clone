using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;
using Unity.Netcode;
[Serializable]
public class ButtonsManager : NetworkBehaviour
{
    [Header("Quests")]
    [SerializeField] GameObject MailBoxCanvas;
    [SerializeField] GameObject MailBoxTexts;
    [SerializeField] GameObject MailManMessege;
    [SerializeField] TextMeshProUGUI textMeshProUGUI;
    private NpcDialouge CurrentNpcDialouge;
    private Dictionary<QuestsManager, TextMeshProUGUI> questsMailBoxDictionary = new Dictionary<QuestsManager, TextMeshProUGUI>();


    private void Start()
    {
        CurrentNpcDialouge = GetComponentInChildren<NpcDialouge>();
         
    }

    #region Buttons
    public void OnMailBoxButtonClick()
    {
        if (!IsOwner)
            return;

        MailBoxCanvas.SetActive(!MailBoxCanvas.activeSelf);
    }

    public void OnExitButtonClick(GameObject CanvasToClose)
    {
        CanvasToClose.SetActive(false);
    }

    public void OnAcceptQuestBtn()
    {
        if (!IsOwner)
            return;

        QuestsManager CurrentQuest = CurrentNpcDialouge.NpcObj.GetComponent<QuestsManager>();
        if (CurrentQuest != null)
        {
            CurrentQuest.activeQuest.AcceptedQuest = true;

            AddQuestToPlayer(CurrentQuest); //Adds The Accepted Quest To The Player Quests List

            AddQuestToMailBox(CurrentQuest);//Adds The Desc of the Quest To The MailBox
        }
    }

    public void AddQuestToPlayer(QuestsManager CurrentQuest)
    {
        GameObject PlayerObj = this.gameObject.transform.root.gameObject;

        PlayerLogic PlayerLogicScript = PlayerObj.GetComponent<PlayerLogic>();

        PlayerLogicScript.activeQuests.Add(CurrentQuest.activeQuest);
    }
    
    public void AddQuestToMailBox(QuestsManager CurrentQuest)
    {
        if (questsMailBoxDictionary.ContainsKey(CurrentQuest))
            return;
        
        TextMeshProUGUI QuestsText = Instantiate(textMeshProUGUI);

        if (!questsMailBoxDictionary.ContainsKey(CurrentQuest))
        { 
            questsMailBoxDictionary.Add(CurrentQuest, QuestsText); //assign each TMpro to its Quest

            QuestsText = CreateNewTmp(QuestsText , CurrentQuest);

            CallMailManMessege();

        }
    }

    public void CallMailManMessege()
    {
        MailManMessege.SetActive(true);
        TypeSentence.Instance.WriteMessege("Quest Has Been Added!");
        StartCoroutine(SetActiveFalse(MailManMessege, 3f));

    }

    public void RemoveQuestFromMailBox(QuestsManager completedQuest)
    {
        // Check if the quest exists in the dictionary
        if (questsMailBoxDictionary.TryGetValue(completedQuest, out TextMeshProUGUI questText))
        {
            Destroy(questText.gameObject);  // Destroy the TextMeshProUGUI GameObject
            questsMailBoxDictionary.Remove(completedQuest);  // Remove the quest from the dictionary
        }
    }   

    public TextMeshProUGUI CreateNewTmp(TextMeshProUGUI QuestText , QuestsManager CurrentQuest)
    {
        //Quest Text Style
        QuestText.fontSize = 20;
        QuestText.color = new Color32(85, 26, 139, 255);  // RGB for #551A8B
        QuestText.alignment = TextAlignmentOptions.Left;
        QuestText.fontStyle = FontStyles.Underline;
        QuestText.AddComponent<MouseHoverText>();
        MouseHoverText MouseHoverTxt = QuestText.GetComponent<MouseHoverText>();
        MouseHoverTxt.questText = QuestText;
        //Quest Text Style

        QuestText.text = "-" + "[" + CurrentQuest.activeQuest.QuestName + "] " + CurrentQuest.activeQuest.description + " " + CurrentQuest.activeQuest.progressCount + "/" + CurrentQuest.activeQuest.objectiveCount;

        Image Background = QuestText.GetComponentInChildren<Image>();

        Image IconImage = Background.transform.GetChild(0).GetComponent<Image>();

        IconImage.sprite = CurrentQuest.Icon;

        IconImage.SetNativeSize();

        QuestText.transform.SetParent(MailBoxTexts.transform, true);

        return QuestText;
    }

    public void UpdateQuestProgress(int newProgress,QuestsManager CurrentQuest)
    {
        if (questsMailBoxDictionary.TryGetValue(CurrentQuest, out TextMeshProUGUI questText))
        {
            int slashIndex = questText.text.LastIndexOf("/");

            if (slashIndex != -1)
            {
                string newProgressText = newProgress.ToString() + "/" + CurrentQuest.activeQuest.objectiveCount;

                questText.text = questText.text.Substring(0, slashIndex - CurrentQuest.activeQuest.progressCount.ToString().Length) + newProgressText;
            }
        }
    }


    public IEnumerator SetActiveFalse(GameObject Messege , float time)
    {
        yield return new WaitForSeconds(time);
        Messege.SetActive(false);
    }
    #endregion
}
