using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // Gọi hàm này để gây sát thương lên player
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"Player took {amount} damage. Current health: {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player has died!");
        // TODO: Thêm hiệu ứng chết, disable movement, respawn, v.v.
    }

    // (Tùy chọn) Thêm hồi máu
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Player healed. Current health: {currentHealth}");
    }

    // (Tùy chọn) Getter để lấy máu còn lại
    public float GetHealth()
    {
        return currentHealth;
    }
}