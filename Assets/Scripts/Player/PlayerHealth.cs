using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI")]
    public Slider healthSlider;
    public Text healthText;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    // Gây sát thương
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player has died!");
    }

    // Hồi máu
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    // Dùng khi player lên level
    public void SetMaxHealth(float newMaxHealth, bool refill = true)
    {
        maxHealth = newMaxHealth;

        if (refill)
            currentHealth = maxHealth; // hồi đầy máu nếu muốn

        UpdateHealthUI(); // luôn cập nhật UI khi max HP thay đổi
    }

    // Cập nhật Slider + Text
    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"HP: {(int)currentHealth}/{(int)maxHealth}";
        }
    }

    public float GetHealth()
    {
        return currentHealth;
    }
}