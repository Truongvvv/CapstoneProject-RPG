using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class PlayerQuestManager : MonoBehaviour
{
    public static PlayerQuestManager Instance;
    public Quest currentQuest;
    private int currentQuestIndex = -1; // Chưa nhận nhiệm vụ

    [Header("Key Quest (Thu thập pha lê)")]
    public TMP_Text keyQuestText;      // Text hiển thị trong QuestTracker
    public int crystalsCollected = 0;
    public int crystalsRequired = 4;
    public bool keyQuestCompleted = false;

    [Header("Quest UI")]
    public TMP_Text questMessageText;  // Bổ sung để tránh lỗi null nếu bạn đang dùng trong UpdateQuestUI

    public static Action OnWinGame;
    
    void Awake()
    {
        Instance = this;
    }

    public Quest activeQuest => currentQuest;

    public bool HasActiveQuest()
    {
        return currentQuest != null && currentQuest.isAccepted && !currentQuest.isCompleted;
    }

    public bool CanTurnIn()
    {
        return currentQuest != null && currentQuest.isCompleted;
    }

    public void AcceptNextQuest(List<Quest> questList)
    {
        if (currentQuestIndex + 1 < questList.Count)
        {
            currentQuestIndex++;
            currentQuest = questList[currentQuestIndex];
            currentQuest.isAccepted = true;
            currentQuest.isCompleted = false;
            currentQuest.currentKills = 0;

            Debug.Log("Get the quest: " + currentQuest.questName);
            QuestTracker.Instance.UpdateTracker(
                currentQuest.questName,
                $"{currentQuest.description} (0/{currentQuest.requiredKills})"
            );
        }
        else
        {
            Debug.Log("No more new missions!");
            QuestTracker.Instance.ClearTracker();
        }
    }

    public int GetCurrentQuestIndex()
    {
        return currentQuestIndex;
    }

    public void AddKill(string enemyTag)
    {
        if (currentQuest != null && currentQuest.isAccepted && !currentQuest.isCompleted)
        {
            if (enemyTag == currentQuest.enemyTag)
            {
                currentQuest.currentKills++;
                QuestTracker.Instance.UpdateTracker(
                    currentQuest.questName,
                    $"{currentQuest.description} ({currentQuest.currentKills}/{currentQuest.requiredKills})"
                );

                if (currentQuest.currentKills >= currentQuest.requiredKills)
                {
                    CompleteQuest();
                }
            }
        }
    }

    void CompleteQuest()
    {
        currentQuest.isCompleted = true;
        QuestTracker.Instance.UpdateTracker(currentQuest.questName, "Completed");
        Debug.Log("Complete the mission: " + currentQuest.questName);
    }

    public void TurnInQuest(List<Quest> questList)
    {
        if (currentQuest != null && currentQuest.isCompleted)
        {
            Debug.Log("Return the task: " + currentQuest.questName);

            PlayerMovement player = FindObjectOfType<PlayerMovement>();
            if (player != null)
            {
                player.GainExp(currentQuest.expReward);
                Debug.Log($"[QUEST] +{currentQuest.expReward} EXP từ {currentQuest.questName}");
            }

            QuestTracker.Instance.ClearTracker();
            currentQuest = null;

            AcceptNextQuest(questList);
        }
    }

    // Cập nhật text trong QuestTracker
    void UpdateQuestUI()
    {
        if (keyQuestText != null)
        {
            if (keyQuestCompleted)
                keyQuestText.text = "Collect Crystals (Complete)";
            else
                keyQuestText.text = $"Collect Crystals ({crystalsCollected}/{crystalsRequired})";
        }
    }

    // Gọi khi nhặt Crystal
    public void CollectCrystal(string crystalID)
    {
        crystalsCollected++;
        UpdateQuestUI();

        if (crystalsCollected >= crystalsRequired)
        {
            UpdateQuestUI();
            keyQuestCompleted = true;
            OnWinGame?.Invoke();
        }
    }
}