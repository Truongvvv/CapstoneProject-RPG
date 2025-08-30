using UnityEngine;
using System.Collections.Generic;

public class PlayerQuestManager : MonoBehaviour
{
    public static PlayerQuestManager Instance;
    public Quest currentQuest;
    private int currentQuestIndex = -1; // Chưa nhận nhiệm vụ

    void Awake()
    {
        Instance = this;
    }

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

            Debug.Log("Nhận nhiệm vụ: " + currentQuest.questName);
            QuestTracker.Instance.UpdateTracker(
                currentQuest.questName,
                $"{currentQuest.description} (0/{currentQuest.requiredKills})"
            );
        }
        else
        {
            Debug.Log("Không còn nhiệm vụ mới!");
            QuestTracker.Instance.ClearTracker();
        }
    }

    public void AddKill(string enemyTag)
    {
        if (currentQuest != null && currentQuest.isAccepted && !currentQuest.isCompleted)
        {
            // Chỉ tăng số lượng nếu tag đúng
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
        QuestTracker.Instance.UpdateTracker(currentQuest.questName, "Đã hoàn thành!");
        Debug.Log("Hoàn thành nhiệm vụ: " + currentQuest.questName);
    }

    public void TurnInQuest(List<Quest> questList)
    {
        if (currentQuest != null && currentQuest.isCompleted)
        {
            Debug.Log("Trả nhiệm vụ: " + currentQuest.questName);
            QuestTracker.Instance.ClearTracker();
            currentQuest = null;

            // Nhận nhiệm vụ tiếp theo (nếu còn)
            AcceptNextQuest(questList);
        }
    }
}