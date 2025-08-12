using UnityEngine;

public class SkillProjectile : MonoBehaviour
{
    public GameObject burnEffectPrefab;
    public float lifeTime = 5f;
    public float initialHitDamage = 50f; // damage khi đạn trúng lần đầu

    void Start()
    {
        Destroy(gameObject, lifeTime); // tự hủy nếu không trúng gì
    }

    void OnTriggerEnter(Collider other)
    {
        SmallEnemyAI enemy = other.GetComponent<SmallEnemyAI>();
        BossAI boss = other.GetComponent<BossAI>();

        Debug.Log("Projectile hit: " + other.name);
        Debug.Log("Enemy found: " + (enemy != null));
        Debug.Log("Boss found: " + (boss != null));
        Debug.Log("Prefab assigned: " + (burnEffectPrefab != null));
        Debug.Log("PlayerCombat.Instance: " + PlayerCombat.Instance);

        if (enemy != null)
        {
            enemy.TakeDamage(initialHitDamage);

            int burnDamage = PlayerCombat.Instance != null ? PlayerCombat.Instance.burnBaseDamage : 0;
            GameObject burn = Instantiate(burnEffectPrefab, enemy.transform.position, Quaternion.identity, enemy.transform);

            SkillBurnProjectile burnScript = burn.GetComponent<SkillBurnProjectile>();
            if (burnScript != null)
                burnScript.Init(enemy, burnDamage);
            else
                Debug.LogError("Burn prefab missing SkillBurnProjectile!");
        }
        else if (boss != null)
        {
            boss.TakeDamage(initialHitDamage);

            int burnDamage = PlayerCombat.Instance != null ? PlayerCombat.Instance.burnBaseDamage : 0;
            GameObject burn = Instantiate(burnEffectPrefab, boss.transform.position, Quaternion.identity, boss.transform);

            SkillBurnProjectile burnScript = burn.GetComponent<SkillBurnProjectile>();
            if (burnScript != null)
                burnScript.Init(boss, burnDamage);
            else
                Debug.LogError("Burn prefab missing SkillBurnProjectile!");
        }

        if (enemy != null || boss != null)
        {
            Destroy(gameObject);
        }
    }
}