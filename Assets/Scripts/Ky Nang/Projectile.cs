using UnityEngine;
using System.Collections;
public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float destroyAfterSeconds = 2f;
    public float damage = 50f;
    private float finalDamage;

    [Header("Target Tags")]
    public string[] targetTags; // Mảng các tag mục tiêu (Enemy, Boss, Dummy...)

    void Start()
    {
        // Lấy buff từ nơi lưu 
        float damageBuff = PlayerCombat.Instance != null ? PlayerCombat.Instance.currentDamageBuff : 0f;

        finalDamage = damage + damageBuff;

        Destroy(gameObject, destroyAfterSeconds);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        SmallEnemyAI enemy = other.GetComponent<SmallEnemyAI>();
        if (enemy != null)
        {
            enemy.TakeDamage(finalDamage); // ← dùng finalDamage
            Debug.Log("Trúng " + other.name + " - Gây damage: " + finalDamage);
            Destroy(gameObject);
            return;
        }

        EnemyAI enemyAI = other.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.TakeDamage(finalDamage);
            Debug.Log("Trúng " + other.name + " - Gây damage: " + finalDamage);
            Destroy(gameObject);
            return;
        }

        BossAI bossAI = other.GetComponent<BossAI>();
        if (bossAI != null)
        {
            bossAI.TakeDamage(finalDamage);
            Debug.Log("Trúng " + other.name + " - Gây damage: " + finalDamage);
            Destroy(gameObject);
            return;
        }
    }

    private bool IsTargetTag(string tag)
    {
        foreach (string target in targetTags)
        {
            if (tag == target) return true;
        }
        return false;
    }
}