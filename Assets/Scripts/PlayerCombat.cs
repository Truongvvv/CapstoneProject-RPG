using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    public int damage = 20;
    public Collider weaponCollider;
    public static PlayerCombat Instance;
    public Animator animator;
    public float buffAmount = 30f;
    public float buffDuration = 5f;

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
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Gọi animation Buff nếu có
            if (animator != null)
                animator.SetTrigger("Buff");

            // Kích hoạt buff damage
            ApplyDamageBuff(buffAmount, buffDuration);
        }
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
        damage += (int)amount; // ← Cộng vào damage gốc
        Debug.Log("Damage buffed: +" + amount);

        yield return new WaitForSeconds(duration);

        currentDamageBuff -= amount;
        damage -= (int)amount; // ← Trừ lại sau khi hết buff
        Debug.Log("Damage buff expired: -" + amount);
    }
}