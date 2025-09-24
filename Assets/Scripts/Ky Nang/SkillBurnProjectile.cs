using UnityEngine;
using System.Collections;

public class SkillBurnProjectile : MonoBehaviour
{
    public int damagePerTick = 200; // Damage mỗi lần đốt
    public float tickInterval = 2f; // Thời gian giữa mỗi lần đốt
    public float duration = 10f;    // Thời gian tồn tại
    public GameObject extraVFXPrefab; // VFX thêm khi đốt

    private float elapsed = 0f;
    private SmallEnemyAI enemy;
    private RoockEnemyAI roock;
    private EnemyAI enemyAI;
    private BossAI boss;
    private BossDragon bossDragon;
    private EnemyDragon dragonEnemy;
    private int burnDamage;

    public void Init(SmallEnemyAI target, int burnDamage)
    {
        enemy = target;
        this.burnDamage = burnDamage;
    }

    public void Init(BossAI target, int burnDamage)
    {
        boss = target;
        this.burnDamage = burnDamage;
    }
    public void Init(BossDragon target, int burnDamage)
    {
        bossDragon = target;
        this.burnDamage = burnDamage;
    }
    public void Init(EnemyAI target, int burnDamage)
    {
        enemyAI = target;
        this.burnDamage = burnDamage;
    }
    public void Init(RoockEnemyAI target, int burnDamage)
    {
        roock = target;
        this.burnDamage = burnDamage;
    }
    public void Init(EnemyDragon target, int burnDamage)
    {
        dragonEnemy = target;
        this.burnDamage = burnDamage;
    }
    private void Start()
    {
        StartCoroutine(DoBurnDamage());
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= duration)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator DoBurnDamage()
    {
        float time = 0f;
        while (time < duration)
        {
            // Gây sát thương
            if (enemy != null)
            {
                enemy.TakeDamage(burnDamage);
                Debug.Log($"[Burn] Enemy bị đốt {burnDamage} damage");
            }
            if (boss != null)
            {
                boss.TakeDamage(burnDamage);
                Debug.Log($"[Burn] Boss bị đốt {burnDamage} damage");
            }
            if (roock != null)
            {
                roock.TakeDamage(burnDamage);
                Debug.Log($"[Burn] rook bị đốt {burnDamage} damage");
            }
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(burnDamage);
                Debug.Log($"[Burn] Rhino bị đốt {burnDamage} damage");
            }
            if (dragonEnemy != null)
            {
                dragonEnemy.TakeDamage(burnDamage);
                Debug.Log($"[Burn] BossDragon bị đốt {burnDamage} damage");
            }
            if (bossDragon != null)
            {
                bossDragon.TakeDamage(burnDamage);
                Debug.Log($"[Burn] BossDragon bị đốt {burnDamage} damage");
            }
            // Spawn VFX màu mè
            if (extraVFXPrefab != null)
            {
                Transform target = enemy != null ? enemy.transform : boss != null ? boss.transform : null;
                if (target != null)
                {
                    GameObject extra = Instantiate(extraVFXPrefab, target.position, Quaternion.identity, target);
                    Destroy(extra, 3f); // VFX tồn tại ngắn để làm hiệu ứng
                }
            }

            time += tickInterval;
            yield return new WaitForSeconds(tickInterval);
        }
        Destroy(gameObject);
    }
}