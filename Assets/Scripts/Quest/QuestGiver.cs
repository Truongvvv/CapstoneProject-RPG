using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class QuestGiver : MonoBehaviour
{
    public List<Quest> quests; // Danh sách các nhiệm vụ
    public GameObject questUI;
    public Button acceptButton;
    public Button completeButton;

    private bool playerInRange;
    private int selectedIndex = 0; // 0 = nhận, 1 = trả

    public int[] expRewards; // mảng phần thưởng exp theo thứ tự quest

    void Start()
    {
        questUI.SetActive(false);
        HighlightButton();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            questUI.SetActive(true);
            selectedIndex = 0;
            HighlightButton();
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
                if (selectedIndex == 0 && !PlayerQuestManager.Instance.HasActiveQuest())
                {
                    PlayerQuestManager.Instance.AcceptNextQuest(quests);
                    questUI.SetActive(false);
                }
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
                        Debug.Log($"[QUEST] Hoàn thành quest {questIndex}, +{reward} EXP!");
                    }

                    questUI.SetActive(false);
                }
            }
        }
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