using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100;
    private float currentHealth;

    [Header("UI")] public Slider healthSlider;
    public Text healthText;

    public static Action OnPlayerDeath;
    public static Action<float, float> UpdateHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealth?.Invoke(currentHealth, maxHealth);
    }

    // Gây sát thương
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealth?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            OnPlayerDeath?.Invoke();
        }
    }

    // Hồi máu
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealth?.Invoke(currentHealth, maxHealth);
    }

    // Dùng khi player lên level
    public void SetMaxHealth(int newMaxHealth, bool refill = true)
    {
        maxHealth = newMaxHealth;

        if (refill)
            currentHealth = maxHealth; // hồi đầy máu nếu muốn

        UpdateHealth?.Invoke(currentHealth, maxHealth);
    }

    public float GetHealth()
    {
        return currentHealth;
    }
}