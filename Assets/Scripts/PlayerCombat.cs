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

    public int burnBaseDamage = 200; // damage cơ bản của VFX đốt


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
        damage += (int)amount;          // tăng melee damage
        burnBaseDamage += (int)amount;  // tăng burn damage
        Debug.Log("Damage buffed: +" + amount);

        yield return new WaitForSeconds(duration);

        currentDamageBuff -= amount;
        damage -= (int)amount;          // giảm melee damage
        burnBaseDamage -= (int)amount;  // giảm burn damage
        Debug.Log("Damage buff expired: -" + amount);
    }
}