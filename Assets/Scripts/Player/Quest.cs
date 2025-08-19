[System.Serializable]
public class Quest
{
    public string questName;      // Tên nhiệm vụ
    public string description;    // Mô tả nhiệm vụ
    public string enemyTag;       // Tag của quái cần tiêu diệt (ví dụ: "Enemy", "Boss")

    public int requiredKills;     // Số lượng cần tiêu diệt
    public int currentKills;      // Số lượng đã tiêu diệt

    public bool isAccepted;       // Đã nhận nhiệm vụ hay chưa
    public bool isCompleted;      // Đã hoàn thành chưa
}