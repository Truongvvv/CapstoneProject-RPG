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
        RoockEnemyAI roock = other.GetComponent<RoockEnemyAI>();
        BossDragon bossDragon = other.GetComponent<BossDragon>();
        EnemyAI enemyAI = other.GetComponent<EnemyAI>();
        EnemyDragon dragonEnemy = other.GetComponent<EnemyDragon>();
        EnemyDragonTwo dragonTwoEnemy = other.GetComponent<EnemyDragonTwo>();


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
        else if (bossDragon != null)
        {
            bossDragon.TakeDamage(initialHitDamage);

            int burnDamage = PlayerCombat.Instance != null ? PlayerCombat.Instance.burnBaseDamage : 0;
            GameObject burn = Instantiate(burnEffectPrefab, bossDragon.transform.position, Quaternion.identity, bossDragon.transform);

            SkillBurnProjectile burnScript = burn.GetComponent<SkillBurnProjectile>();
            if (burnScript != null)
                burnScript.Init(bossDragon, burnDamage);
            else
                Debug.LogError("Burn prefab missing SkillBurnProjectile!");
        }
        else if (enemyAI != null)
        {
            enemyAI.TakeDamage(initialHitDamage);

            int burnDamage = PlayerCombat.Instance != null ? PlayerCombat.Instance.burnBaseDamage : 0;
            GameObject burn = Instantiate(burnEffectPrefab, enemyAI.transform.position, Quaternion.identity, enemyAI.transform);

            SkillBurnProjectile burnScript = burn.GetComponent<SkillBurnProjectile>();
            if (burnScript != null)
                burnScript.Init(enemyAI, burnDamage);
            else
                Debug.LogError("Burn prefab missing SkillBurnProjectile!");
        }
        if (roock != null)
        {
            roock.TakeDamage(initialHitDamage);

            int burnDamage = PlayerCombat.Instance != null ? PlayerCombat.Instance.burnBaseDamage : 0;
            GameObject burn = Instantiate(burnEffectPrefab, roock.transform.position, Quaternion.identity, roock.transform);

            SkillBurnProjectile burnScript = burn.GetComponent<SkillBurnProjectile>();
            if (burnScript != null)
                burnScript.Init(roock, burnDamage);
            else
                Debug.LogError("Burn prefab missing SkillBurnProjectile!");
        }

        if (dragonEnemy != null)
        {
            dragonEnemy.TakeDamage(initialHitDamage);

            int burnDamage = PlayerCombat.Instance != null ? PlayerCombat.Instance.burnBaseDamage : 0;
            GameObject burn = Instantiate(burnEffectPrefab, dragonEnemy.transform.position, Quaternion.identity, dragonEnemy.transform);

            SkillBurnProjectile burnScript = burn.GetComponent<SkillBurnProjectile>();
            if (burnScript != null)
                burnScript.Init(dragonEnemy, burnDamage);
            else
                Debug.LogError("Burn prefab missing SkillBurnProjectile!");
        }
        if (dragonTwoEnemy != null)
        {
            dragonTwoEnemy.TakeDamage(initialHitDamage);

            int burnDamage = PlayerCombat.Instance != null ? PlayerCombat.Instance.burnBaseDamage : 0;
            GameObject burn = Instantiate(burnEffectPrefab, dragonTwoEnemy.transform.position, Quaternion.identity, dragonTwoEnemy.transform);

            SkillBurnProjectile burnScript = burn.GetComponent<SkillBurnProjectile>();
            if (burnScript != null)
                burnScript.Init(dragonTwoEnemy, burnDamage);
            else
                Debug.LogError("Burn prefab missing SkillBurnProjectile!");
        }

        if (enemy != null || boss != null || bossDragon != null || enemyAI != null || roock != null || dragonEnemy != null || dragonTwoEnemy != null)
        {
            Destroy(gameObject);
        }
    }
}