using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public enum Quests
{
    KillQuest,
    PickUpQuest,

}

public class QuestsManager : MonoBehaviour
{
    [SerializeReference]
    [Header("Active Quest")]
    public QuestsBase activeQuest;

    [Header("Quest Type")]
    public Quests questType;

    [Header("Icon")]
    public Sprite Icon;


    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            activeQuest.PlayerThatTalked = collision.transform.root.gameObject;

            if (this != null)
            {
                activeQuest.CurrentQuest = this;

            }
        }
       
    }

    private void OnValidate()
    {
        // Switch quest type in the editor
        switch (questType)
        {
            case Quests.KillQuest:
                if (!(activeQuest is KillQuest))
                {
                    activeQuest = new KillQuest();
                }
                break;

            case Quests.PickUpQuest:
                if (!(activeQuest is PickUpQuest))
                {
                    activeQuest = new PickUpQuest();
                }
                break;

                // Add more cases for other quest types
        }


       

       

    }
[System.Serializable]
public class QuestsBase
{
    public string QuestName;
    public string description;
    public int objectiveCount;
    [HideInInspector] public int progressCount = 0;
    public bool isCompleted;
    public bool AcceptedQuest;
    public int reward;
    [HideInInspector]  public GameObject PlayerThatTalked;
    [HideInInspector] public QuestsManager CurrentQuest;

    public event Action<QuestsBase> OnQuestCompleted;

        protected void CompleteQuest()
        {
            if (!isCompleted)  // Prevent multiple completions
            {
                isCompleted = true;
                TypeSentence.Instance.CompleteQuest(QuestName);
                if (PlayerThatTalked != null)
                {
                    ButtonsManager buttonsManager = PlayerThatTalked.GetComponentInChildren<ButtonsManager>();
                    buttonsManager.RemoveQuestFromMailBox(CurrentQuest);
                    Destroy(CurrentQuest.gameObject);
                }
                else
                    Debug.LogError("Player is null");

                OnQuestCompleted?.Invoke(this);
            }
        }

        protected void UpdateText(int newCount)
        {
            ButtonsManager buttonsManager = PlayerThatTalked.GetComponentInChildren<ButtonsManager>();
            buttonsManager.UpdateQuestProgress(newCount,CurrentQuest);  
        }

    }

    [System.Serializable]

    public class KillQuest : QuestsBase
    {
        public EnemyType enemyToKill;
       

        public void KilledEnemies(bool EnemyIsDead)
        {
            

                if (EnemyIsDead)
                {
                    progressCount++;
                    UpdateText(progressCount);
                    if (progressCount >= objectiveCount)
                    {
                      CompleteQuest();
                    }
            }
            
        }


    }

    public class PickUpQuest:QuestsBase
    {
        public Item ItemToCollect;
        
        public  void PickedUpItem(bool PicekdUpItem)
        {
            
                progressCount++;
               UpdateText(progressCount);

                if (progressCount >= objectiveCount)
                {
                  CompleteQuest();
                }
            
        }
    }
}

