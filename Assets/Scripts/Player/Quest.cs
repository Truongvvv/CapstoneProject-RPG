using UnityEngine;

[System.Serializable]
public class Quest
{
    public string questName;      // Tên nhiệm vụ
    public string description;    // Mô tả nhiệm vụ
    public string enemyTag;       // Tag của quái cần tiêu diệt

    public int requiredKills;     // Số lượng cần tiêu diệt
    [HideInInspector] public int currentKills;      // Số lượng đã tiêu diệt

    [HideInInspector] public bool isAccepted;       // Đã nhận nhiệm vụ hay chưa
    [HideInInspector] public bool isCompleted;      // Đã hoàn thành chưa

    [Header("Reward")]
    public int expReward;         // EXP thưởng khi hoàn thành
}