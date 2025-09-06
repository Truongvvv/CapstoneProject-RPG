using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("Damage Stats")]
    public int damage = 20;            // dame hiện tại (sẽ scale theo buff và level)
    public int burnBaseDamage = 200;   // damage cơ bản của VFX đốt

    [HideInInspector] public int baseMeleeDamage;   // damage gốc gậy (dùng cho level up)
    [HideInInspector] public int baseBurnDamage;    // damage gốc burn (dùng cho level up)

    public Collider weaponCollider;
    public static PlayerCombat Instance;
    public Animator animator;

    [Header("Buff Settings")]
    public float buffAmount = 30f;
    public float buffDuration = 5f;

    public float currentDamageBuff = 0f;

    void Awake()
    {
        Instance = this;

        // Ghi nhớ damage gốc để cộng thêm khi level up
        baseMeleeDamage = damage;
        baseBurnDamage = burnBaseDamage;
    }

    void Start()
    {
        if (weaponCollider != null)
            weaponCollider.enabled = false; // Luôn tắt khi bắt đầu
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (animator != null)
                animator.SetTrigger("Buff");

            ApplyDamageBuff(buffAmount, buffDuration);
        }
    }

    // Animation Event: bắt đầu hitbox
    public void MeleeAttackStart()
    {
        Debug.Log("Melee Attack Start!");
        if (weaponCollider != null)
            weaponCollider.enabled = true;
    }

    // Animation Event: tắt hitbox
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
        damage += (int)amount;
        burnBaseDamage += (int)amount;
        Debug.Log("Damage buffed: +" + amount);

        yield return new WaitForSeconds(duration);

        currentDamageBuff -= amount;
        damage -= (int)amount;
        burnBaseDamage -= (int)amount;
        Debug.Log("Damage buff expired: -" + amount);
    }

    // Gọi khi Level Up (tăng vĩnh viễn)
    public void AddPermanentDamage(int meleeBonus, int burnBonus)
    {
        baseMeleeDamage += meleeBonus;
        baseBurnDamage += burnBonus;

        damage = baseMeleeDamage + (int)currentDamageBuff;
        burnBaseDamage = baseBurnDamage + (int)currentDamageBuff;

        Debug.Log($"[LEVEL UP BONUS] New Melee Damage = {damage}, Burn Damage = {burnBaseDamage}");
    }
}