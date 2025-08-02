using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    public int damage = 20;
    public Collider weaponCollider;
    public static PlayerCombat Instance;

    public float currentDamageBuff = 0f; 

    void Start()
    {
        if (weaponCollider != null)
            weaponCollider.enabled = false; // Luôn tắt khi bắt đầu
    }
    void Awake()
    {
        Instance = this;
    }

    // Gọi từ animation event để bắt đầu gây damage
    public void MeleeAttackStart()
    {
        Debug.Log("Melee Attack Start!");
        if (weaponCollider != null)
            weaponCollider.enabled = true;
    }

    // Gọi từ animation event để kết thúc gây damage
    public void MeleeAttackEnd()
    {
        Debug.Log("Melee Attack End!");
        if (weaponCollider != null)
            weaponCollider.enabled = false;
    }

    public void ApplyDamageBuff(float amount, float duration)
    {
        StartCoroutine(DamageBuffCoroutine(amount, duration));
    }

    private IEnumerator DamageBuffCoroutine(float amount, float duration)
    {
        currentDamageBuff += amount;
        Debug.Log("Damage buffed: +" + amount);
        yield return new WaitForSeconds(duration);
        currentDamageBuff -= amount;
        Debug.Log("Damage buff expired: -" + amount);
    }
}