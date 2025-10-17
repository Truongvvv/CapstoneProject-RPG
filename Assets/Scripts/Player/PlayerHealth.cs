using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100;
    private float currentHealth;
    private bool isDead = false;

    [Header("UI References")]
    public Slider healthSlider;
    public Text healthText;

    public static Action OnPlayerDeath;
    public static Action<float, float> UpdateHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            isDead = true;
            OnPlayerDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    public void SetMaxHealth(int newMaxHealth, bool refill = true)
    {
        maxHealth = newMaxHealth;
        if (refill)
            currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public float GetHealth() => currentHealth;

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth / maxHealth;

        if (healthText != null)
            healthText.text = $"{Mathf.RoundToInt(currentHealth)} / {Mathf.RoundToInt(maxHealth)}";

        UpdateHealth?.Invoke(currentHealth, maxHealth);
    }
}