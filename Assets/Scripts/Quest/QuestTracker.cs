using UnityEngine;
using TMPro;

public class QuestTracker : MonoBehaviour
{
    public static QuestTracker Instance;
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI questDescriptionText;

    void Awake()
    {
        Instance = this;
    }

    public void UpdateTracker(string questName, string description)
    {
        if (questNameText != null) questNameText.text = questName;
        if (questDescriptionText != null) questDescriptionText.text = description;
    }

    public void ClearTracker()
    {
        if (questNameText != null) questNameText.text = "";
        if (questDescriptionText != null) questDescriptionText.text = "";
    }
}