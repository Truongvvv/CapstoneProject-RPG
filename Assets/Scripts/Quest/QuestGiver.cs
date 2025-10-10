using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class QuestGiver : MonoBehaviour
{
    [Header("Quest Settings")]
    public List<Quest> quests; // Danh sách các nhiệm vụ
    public int[] expRewards;   // Mảng phần thưởng EXP theo thứ tự quest

    [Header("UI Elements")]
    public GameObject questUI;
    public Button acceptButton;
    public Button completeButton;

    public TextMeshProUGUI mainQuestText;
    public TextMeshProUGUI keyQuestText;

    private bool playerInRange;
    private int selectedIndex = 0; // 0 = nhận, 1 = trả

    void Start()
    {
        questUI.SetActive(false);
        HighlightButton();
        UpdateQuestUI(); // Cập nhật lần đầu
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            questUI.SetActive(true);
            selectedIndex = 0;
            HighlightButton();
            UpdateQuestUI(); // Cập nhật khi mở UI
        }

        if (questUI.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                selectedIndex = Mathf.Max(0, selectedIndex - 1);
                HighlightButton();
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                selectedIndex = Mathf.Min(1, selectedIndex + 1);
                HighlightButton();
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                // Nhận quest
                if (selectedIndex == 0 && !PlayerQuestManager.Instance.HasActiveQuest())
                {
                    PlayerQuestManager.Instance.AcceptNextQuest(quests);
                    UpdateQuestUI();
                    questUI.SetActive(false);
                }
                // Trả quest
                else if (selectedIndex == 1 && PlayerQuestManager.Instance.CanTurnIn())
                {
                    int questIndex = PlayerQuestManager.Instance.GetCurrentQuestIndex();
                    PlayerQuestManager.Instance.TurnInQuest(quests);

                    // ---- Thưởng EXP theo mảng ----
                    PlayerMovement player = FindObjectOfType<PlayerMovement>();
                    if (player != null)
                    {
                        int reward = 200; // mặc định
                        if (expRewards != null && questIndex < expRewards.Length)
                            reward = expRewards[questIndex];

                        player.GainExp(reward);
                        Debug.Log($"[QUEST] Complete quest {questIndex}, +{reward} EXP!");
                    }

                    UpdateQuestUI();
                    questUI.SetActive(false);
                }
            }
        }

        // Cập nhật UI liên tục (hiển thị tiến độ key quest)
        if (questUI.activeSelf)
            UpdateQuestUI();
    }

    void UpdateQuestUI()
    {
        var qm = PlayerQuestManager.Instance;
        if (qm == null) return;

        // Hiện Main Quest
        if (qm.HasActiveQuest())
        {
            Quest quest = qm.activeQuest;
            mainQuestText.text = $"Main Quest: {quest.questName} ({quest.currentKills}/{quest.requiredKills})";
        }
        else
        {
            mainQuestText.text = "";
        }

        // Hiện Key Quest
        string keyQuestStatus = qm.keyQuestCompleted ? "Complete"
                                                     : $"{qm.crystalsCollected}/{qm.crystalsRequired}";
        keyQuestText.text = $"Key Quest: collection Crystal ({keyQuestStatus})";
    }

    void HighlightButton()
    {
        Color normal = Color.yellow;
        Color selected = Color.green;

        acceptButton.GetComponent<Image>().color = (selectedIndex == 0) ? selected : normal;
        completeButton.GetComponent<Image>().color = (selectedIndex == 1) ? selected : normal;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            questUI.SetActive(false);
        }
    }
}