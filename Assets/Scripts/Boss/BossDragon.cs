using UnityEngine;
using UnityEngine.AI;

public class BossDragon : MonoBehaviour
{
    [Header("References")]
    public Transform player; // Cache trong Start() để tránh FindObjectOfType
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;

    private NavMeshAgent agent;
    private Animator animator;

    [Header("Stats")]
    public float detectionRange = 25f;
    public float attackRange = 3f;
    public float retreatRange = 6f;
    public float health = 500f;
    public float retreatThreshold = 100f;
    public int expReward = 200;

    [Header("Attack Settings")]
    public float attackRadius = 2f;
    public float attackCooldown = 1.5f;
    private float attackTimer;
    private bool hasDealtDamage = false;
    public LayerMask playerLayer;

    [Header("Roar Settings")]
    public float roarDuration = 3f;
    private float roarTimer;
    private bool hasRoared;
    private bool isRoaring;

    [Header("VFX Settings")]
    public GameObject chargeVFXPrefab;
    public Transform vfxSpawnPoint;
    private GameObject activeChargeVFX; // Dùng pooling thì tốt hơn

    [Header("Fly Settings")]
    public float flyDuration = 5f;
    private float flyTimer;
    private bool hasFlown;

    [Header("Special Skill Settings")]
    public float specialSkillCooldown = 10f;
    public float specialSkillDuration = 3f;
    public float specialSkillRadius = 5f;
    public float specialSkillDamage = 150f;
    public GameObject specialSkillVFX;

    private float combatTimer;
    private bool isUsingSpecialSkill;
    private float specialSkillTimer;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip roarSFX;

    private float pathUpdateInterval = 0.3f; // update path mỗi 0.3s
    private float pathTimer = 0f;

    private Vector3 originalPosition;

    private enum State { Idle, Fly, Roar, Chase, Attack, Retreat, Return, SpecialSkill }
    private State currentState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        currentState = State.Idle;
        originalPosition = transform.position;

        // Cache Player references (tránh FindObjectOfType nhiều lần)
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerMovement = player.GetComponent<PlayerMovement>();
        }

        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;
    }

    void Update()
    {
        if (player == null) return;

        // Dùng sqrMagnitude thay Distance (nhanh hơn vì tránh căn bậc 2)
        float sqrDistToPlayer = (transform.position - player.position).sqrMagnitude;
        float sqrAttackRange = attackRange * attackRange;
        float sqrDetectRange = detectionRange * detectionRange;
        float sqrRetreatRange = retreatRange * retreatRange;

        // FSM điều kiện
        if (health <= retreatThreshold && sqrDistToPlayer < sqrRetreatRange)
        {
            currentState = State.Retreat;
        }
        else if (sqrDistToPlayer <= sqrAttackRange)
        {
            currentState = State.Attack;
        }
        else if (sqrDistToPlayer <= sqrDetectRange)
        {
            if (!hasFlown)
            {
                currentState = State.Fly;
            }
            else if (!hasRoared || isRoaring)
            {
                currentState = State.Roar;
            }
            else
            {
                currentState = State.Chase;
            }

            // Combat timer chỉ tính khi Chase/Attack
            if (currentState == State.Chase || currentState == State.Attack)
            {
                combatTimer += Time.deltaTime;
                if (combatTimer >= specialSkillCooldown && !isUsingSpecialSkill)
                {
                    currentState = State.SpecialSkill;
                }
            }
        }
        else
        {
            // Reset khi player ra ngoài detection
            combatTimer = 0f;
            hasRoared = false;
            roarTimer = 0f;

            // Quay về vị trí ban đầu
            if ((transform.position - originalPosition).sqrMagnitude > 0.5f * 0.5f)
                currentState = State.Return;
            else
                currentState = State.Idle;
        }

        // Reset damage khi không attack
        if (currentState != State.Attack)
        {
            hasDealtDamage = false;
            attackTimer = 0f;
        }

        UpdateState(sqrDistToPlayer);
    }

    void UpdateState(float sqrDistToPlayer)
    {
        switch (currentState)
        {
            case State.Idle:
                StopAgent();
                SetAnim("isRunning", false);
                SetAnim("isAttacking", false);
                break;

            case State.Fly:
                HandleFly();
                break;

            case State.Roar:
                HandleRoar();
                break;

            case State.Chase:
                HandleChase();
                break;

            case State.Attack:
                HandleAttack();
                break;

            case State.Retreat:
                HandleRetreat();
                break;

            case State.Return:
                HandleReturn();
                break;

            case State.SpecialSkill:
                HandleSpecialSkill(sqrDistToPlayer);
                break;
        }
    }

    #region State Handlers

    void HandleFly()
    {
        if (agent.enabled) agent.enabled = false;
        transform.LookAt(player);

        if (!hasFlown && flyTimer == 0f)
        {
            animator.SetTrigger("FlyUp");
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        flyTimer += Time.deltaTime;

        if (flyTimer >= flyDuration)
        {
            hasFlown = true;
            flyTimer = 0f;

            agent.enabled = true;
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = true;

            currentState = State.Roar;
        }
    }

    void HandleRoar()
    {
        isRoaring = true;
        StopAgent();

        Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookPos);

        SetAnim("isRunning", false);
        SetAnim("isAttacking", false);

        if (!hasRoared && activeChargeVFX == null)
        {
            animator.SetTrigger("Roar");
            hasRoared = true;
            PlaySound(roarSFX);

            if (chargeVFXPrefab != null && vfxSpawnPoint != null)
            {
                activeChargeVFX = Instantiate(chargeVFXPrefab, vfxSpawnPoint.position, Quaternion.identity, vfxSpawnPoint);
            }
        }

        roarTimer += Time.deltaTime;
        if (roarTimer >= roarDuration)
        {
            isRoaring = false;
            currentState = State.Chase;
            roarTimer = 0f;

            ResumeAgent();

            if (activeChargeVFX != null)
            {
                Destroy(activeChargeVFX);
                activeChargeVFX = null;
            }
        }
    }

    void HandleChase()
    {
        ResumeAgent();
        pathTimer += Time.deltaTime;

        if (pathTimer >= pathUpdateInterval)
        {
            agent.SetDestination(player.position);
            pathTimer = 0f;
        }

        SetAnim("isRunning", true);
        SetAnim("isAttacking", false);
    }

    void HandleAttack()
    {
        StopAgent();
        transform.LookAt(player);

        animator.applyRootMotion = false;
        SetAnim("isRunning", false);
        SetAnim("isAttacking", true);

        attackTimer += Time.deltaTime;
    }

    void HandleRetreat()
    {
        ResumeAgent();
        Vector3 retreatDir = (transform.position - player.position).normalized;
        Vector3 retreatTarget = transform.position + retreatDir * 6f;
        agent.SetDestination(retreatTarget);

        SetAnim("isRunning", true);
        SetAnim("isAttacking", false);
    }

    void HandleReturn()
    {
        ResumeAgent();
        agent.SetDestination(originalPosition);

        SetAnim("isRunning", true);
        SetAnim("isAttacking", false);
    }

    void HandleSpecialSkill(float sqrDistToPlayer)
    {
        if (!isUsingSpecialSkill)
        {
            isUsingSpecialSkill = true;
            specialSkillTimer = 0f;

            StopAgent();
            animator.SetTrigger("SpecialSkill");

            // Spawn VFX (nên pooling thay vì instantiate/destroy)
            if (specialSkillVFX != null)
            {
                GameObject vfx = Instantiate(specialSkillVFX, transform.position, Quaternion.identity, transform);
                Destroy(vfx, specialSkillDuration);
            }

            // Damage trực tiếp player thay vì OverlapSphere (vì chỉ có 1 player)
            if (sqrDistToPlayer <= specialSkillRadius * specialSkillRadius)
            {
                playerHealth?.TakeDamage(specialSkillDamage);
            }
        }

        specialSkillTimer += Time.deltaTime;
        if (specialSkillTimer >= specialSkillDuration)
        {
            isUsingSpecialSkill = false;
            combatTimer = 0f;

            // Quay lại FSM bình thường
            if (sqrDistToPlayer <= attackRange * attackRange)
                currentState = State.Attack;
            else if (sqrDistToPlayer <= detectionRange * detectionRange)
                currentState = State.Chase;
            else
                currentState = State.Return;
        }
    }

    #endregion

    #region Combat & Damage

    public void DealDamage()
    {
        if (player == null || playerHealth == null) return;

        // Kiểm tra thẳng khoảng cách, bỏ OverlapSphere cho nhẹ
        float sqrDist = (player.position - (transform.position + transform.forward * 1f)).sqrMagnitude;
        if (sqrDist <= attackRadius * attackRadius)
        {
            playerHealth.TakeDamage(100f);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0) Die();
    }

    void Die()
    {
        if (PlayerQuestManager.Instance != null)
            PlayerQuestManager.Instance.AddKill(gameObject.tag);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        StopAgent();
        SetAnim("isRunning", false);
        SetAnim("isAttacking", false);
        animator.SetTrigger("Die");

        this.enabled = false;

        // EXP reward (đã cache playerMovement)
        playerMovement?.GainExp(expReward);

        Destroy(gameObject, 8f);
    }

    #endregion

    #region Utils

    private void StopAgent()
    {
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
    }

    private void ResumeAgent()
    {
        if (agent.enabled) agent.isStopped = false;
    }

    private void SetAnim(string param, bool value)
    {
        if (animator.GetBool(param) != value)
            animator.SetBool(param, value);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1.5f, attackRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, specialSkillRadius);
    }

    #endregion
}