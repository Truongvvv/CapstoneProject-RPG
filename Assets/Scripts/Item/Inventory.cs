using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    public int healthPotionCount = 0;
    public Text healthPotionText;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        // Nhấn phím số 1 để dùng bình máu
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UseHealthPotion();
        }
    }

    public void AddHealthPotion(int amount)
    {
        healthPotionCount += amount;
        UpdateUI();
    }

    public void UseHealthPotion()
    {
        if (healthPotionCount > 0)
        {
            PlayerHealth player = FindObjectOfType<PlayerHealth>();
            if (player != null)
            {
                player.Heal(50f); // hồi 50 máu
            }

            healthPotionCount--; // trừ 1 bình
            UpdateUI();
        }
        else
        {
            Debug.Log("Không còn bình máu!");
        }
    }

    void UpdateUI()
    {
        if (healthPotionText != null)
        {
            healthPotionText.text = "x" + healthPotionCount.ToString();
        }
    }
}