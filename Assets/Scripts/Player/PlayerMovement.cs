using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class PlayerMovement : MonoBehaviour
{
    [Header("Di chuyển")] public float moveSpeed = 6f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Dash")] public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    public float gunDamage = 10f;

    [Header("Ground Check")] public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Camera & Model")] public Transform cameraTransform; // Gắn MainCamera
    public Transform modelTransform; // Gắn object là model (có Animator)

    [Header("Combat")] public GameObject staff; // Gậy (hiện/ẩn)
    public float staffHideDelay = 5f; // 5s không dùng thì ẩn
    private int comboStep = 0; // 1 → 2 → 3
    private int requestedComboStep = 0; // combo player muốn (tăng theo lần nhấn)
    private bool isAttacking = false;
    private float staffTimer = 0f; // Đếm ngược để ẩn gậy

    [Header("Level System")] public int level = 1;
    public int currentExp = 0;
    public int expToNextLevel = 100;

    public int maxHP = 100;
    public int currentHP;

    [Header("Combo VFX")] public GameObject[] comboVFX; // Gắn 3 hiệu ứng tương ứng combo 1, 2, 3
    public Transform vfxSpawnPointProjectile; // Vị trí spawn VFX dạng bay

    [Header("VFX")] public Transform vfxSpawnPoint; // Gắn điểm đầu gậy (vị trí spawn)

    [Header("VFX Đạn Bay")] public GameObject projectileVFXPrefab; // Prefab viên đạn visual bay ra
    public float projectileVisualSpeed = 50f; // Tốc độ bay (tùy chỉnh)

    [Header("VFX Buff Shooting")] public GameObject buffedProjectileVFXPrefab; // Prefab đạn khi buff
    public GameObject buffedHitEffectPrefab; // Prefab nổ khi buff

    [Header("Âm thanh di chuyển")] public AudioClip[] footstepClips; // danh sách âm bước chân khi đi thường
    public AudioClip[] sprintClips; // danh sách âm khi chạy nhanh
    public float footstepInterval = 0.5f; // thời gian giữa 2 bước (s)
    private float footstepTimer = 0f;

    private CharacterController controller;
    private Animator animator;

    public static Action OnPauseGame;

    private Vector3 velocity;
    private bool isGrounded;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    public float sprintBonus = 5f; // Tăng thêm khi giữ Q

    [SerializeField] private Transform firePoint; // Gắn điểm bắn (nòng súng) trong Inspector

    [Header("Hiệu ứng")] public GameObject hitEffectPrefab; // Prefab hiệu ứng trúng đạn (tùy chọn)
    public Animator gunAnimator; // Gắn animator từ model/súng
    public float projectileForce = 500f; // Lực đẩy ra trước

    private float originalGunDamage; // Ghi nhớ damage gốc
    private float buffTimer = 0f; // Đếm thời gian buff

    [Header("Buff")] public float buffDamageAmount = 30f;
    public float buffDuration = 10f;
    public GameObject buffAuraVFX; // Hiệu ứng buff quanh người
    private GameObject currentAura;

    private bool isBuffed = false;

    [Header("Combo VFX Buff Mode")] public GameObject[] buffedComboVFX; // Gắn 3 cái tương ứng
    public float buffVFXProjectileForce = 5f; // Lực bay

    public Transform[] buffedComboVFXDirections; // size 3

    [Header("Skill V")] public GameObject skillProjectilePrefab; // Prefab đạn kỹ năng
    public Transform skillSpawnPoint; // Vị trí xuất phát
    public float skillProjectileSpeed = 20f; // Tốc độ bay
    public GameObject burnEffectPrefab; // Prefab hiệu ứng burn (gây dame theo thời gian)

    [Header("Skill Cooldowns")] public float buffFCooldown = 10f; // F hồi chiêu 10s
    public float skillVCooldown = 40f; // V hồi chiêu 40s

    private float buffFCooldownTimer = 0f;
    private float skillVCooldownTimer = 0f;

    [Header("Âm thanh")] public AudioSource audioSource; // gắn AudioSource (thường gắn vào Player hoặc model)
    public AudioClip shootSFX;
    public AudioClip combo1SFX;
    public AudioClip combo2SFX;
    public AudioClip combo3SFX;
    public AudioClip buffSFX;
    public AudioClip skillVSFX;

    [SerializeField] private PlayerUI _playerUI;
    public static bool IsPaused = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = modelTransform.GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;

        var data = DataManager.CurrentData ?? new PlayerData();

        level = data.playerLevel;
        currentExp = data.playerExp;
        expToNextLevel = 100 + (level - 1) * 50;
        maxHP = data.playerMaxHP;
        currentHP = data.playerCurrentHP;
        gunDamage = data.playerDamage;
        moveSpeed = data.moveSpeed;

        Vector3 pos = new Vector3(data.positionX, data.positionY, data.positionZ);
        if (pos != Vector3.zero)
            transform.position = pos;

        IsPaused = false;
        
        _playerUI?.SetUpHealth(currentHP, maxHP);
        _playerUI?.UpdateHealth(currentHP, maxHP);
    }

    void Awake()
    {
        originalGunDamage = gunDamage;
    }

    public void ApplyDamageBuff(float bonusDamage, float duration)
    {
        gunDamage = originalGunDamage + bonusDamage;
        buffTimer = duration;
        Debug.Log($"[BUFF] Tăng sát thương lên {gunDamage} trong {duration} giây.");
    }

    void Update()
    {
        if (IsPaused)
            return;

        if (Input.GetKeyDown(KeyCode.P))
        {
            IsPaused = true;
            OnPauseGame?.Invoke();
        }

        if (Input.GetMouseButtonDown(0)) // Chuột trái
        {
            Shoot();
        }

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (animator != null)
                animator.SetBool("isJumping", true); // Bắt đầu Jump
        }


        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;

            if (animator != null)
                animator.SetBool("isJumping", false); // Kết thúc Jump khi chạm đất
        }

        // Lấy input
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Tính hướng dựa theo camera
        Vector3 camForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = cameraTransform.right;
        Vector3 moveInput = (camForward * v + camRight * h).normalized;

        // DASH
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0f && moveInput != Vector3.zero)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;

            if (animator != null)
                animator.SetBool("isDashing", true); // Bắt đầu Dash
        }

        if (isDashing)
        {
            controller.Move(moveInput * dashSpeed * Time.deltaTime);
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                if (animator != null)
                    animator.SetBool("isDashing", false); // Kết thúc Dash
            }
        }
        else
        {
            bool isSprinting = Input.GetKey(KeyCode.Q) && moveInput.magnitude > 0f;
            float currentSpeed = isSprinting ? moveSpeed + sprintBonus : moveSpeed;

            controller.Move(moveInput * currentSpeed * Time.deltaTime);

            // --- Footstep sound ---
            if (isGrounded && moveInput.magnitude > 0.1f && !isDashing)
            {
                footstepTimer -= Time.deltaTime;

                // chỉ phát khi đã đi qua khoảng thời gian và player đang thật sự di chuyển
                if (footstepTimer <= 0f && controller.velocity.magnitude > 0.5f)
                {
                    PlayFootstep(isSprinting);

                    // reset thời gian giữa 2 bước (ngắn hơn khi chạy)
                    footstepTimer = isSprinting ? footstepInterval * 0.6f : footstepInterval;
                }
            }
            else
            {
                // reset timer khi dừng lại để tránh bị lặp lúc mới di chuyển
                footstepTimer = 0f;
            }
        }


        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        //  Quay model theo hướng di chuyển (KHÔNG quay camera)
        if (moveInput != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveInput);
            modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, targetRot, Time.deltaTime * 10f);
        }

        // Gửi tốc độ  Animator
        if (animator != null)
        {
            animator.SetFloat("Speed", moveInput.magnitude);
            animator.SetBool("isSprinting", Input.GetKey(KeyCode.Q) && moveInput.magnitude > 0f);
        }

        // Cập nhật cooldown dash
        dashCooldownTimer -= Time.deltaTime;

        HandleComboAttack();

        // Theo dõi thời gian buff sát thương
        if (buffTimer > 0f)
        {
            buffTimer -= Time.deltaTime;
            if (buffTimer <= 0f)
            {
                gunDamage = originalGunDamage;
                Debug.Log("[BUFF] Hết thời gian, sát thương trở lại: " + gunDamage);
            }
        }

        //buff F
        if (Input.GetKeyDown(KeyCode.F) && !isBuffed && buffFCooldownTimer <= 0f)
        {
            StartCoroutine(BuffRoutine());
            if (_playerUI) _playerUI.UseSkill_F();
            buffFCooldownTimer = buffFCooldown;
        }

        //buf v
        if (Input.GetKeyDown(KeyCode.V) && skillVCooldownTimer <= 0f)
        {
            ShootSkillV();
            if (_playerUI) _playerUI.UseSkill_V();
            skillVCooldownTimer = skillVCooldown;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (_playerUI) _playerUI.OnQuestButtonPressed();
        }

        // Giảm cooldown
        buffFCooldownTimer -= Time.deltaTime;
        skillVCooldownTimer -= Time.deltaTime;
    }


    void Shoot()
    {
        if (gunAnimator != null)
        {
            gunAnimator.SetTrigger("Fire");
            PlaySound(shootSFX);
        }

        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint is not assigned!");
            return;
        }

        Ray ray = new Ray(firePoint.position, firePoint.forward);
        RaycastHit hit;

        // --- CHỌN TAG THEO TRẠNG THÁI BUFF ---
        string projectileTag = isBuffed ? "BuffedProjectile" : "Projectile";
        string hitTag = isBuffed ? "BuffedHit" : "Hit";

        // Spawn projectile VFX từ pool
        GameObject bulletVFX =
            PoolingManager.Instance.SpawnFromPool(projectileTag, firePoint.position, firePoint.rotation);

        if (Physics.Raycast(ray, out hit, 100f))
        {
            // Gây damage
            EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
            if (enemy != null) enemy.TakeDamage(gunDamage);

            SmallEnemyAI chomper = hit.collider.GetComponent<SmallEnemyAI>();
            if (chomper != null) chomper.TakeDamage(gunDamage);

            BossAI bosslion = hit.collider.GetComponent<BossAI>();
            if (bosslion != null) bosslion.TakeDamage(gunDamage);

            BossDragon bossDragon = hit.collider.GetComponent<BossDragon>();
            if (bossDragon != null) bossDragon.TakeDamage(gunDamage);

            RoockEnemyAI rookEnemy = hit.collider.GetComponent<RoockEnemyAI>();
            if (rookEnemy != null) rookEnemy.TakeDamage(gunDamage);

            EnemyDragon dragonEnemy = hit.collider.GetComponent<EnemyDragon>();
            if (dragonEnemy != null) dragonEnemy.TakeDamage(gunDamage);

            EnemyDragonTwo dragonTwoEnemy = hit.collider.GetComponent<EnemyDragonTwo>();
            if (dragonTwoEnemy != null) dragonTwoEnemy.TakeDamage(gunDamage);

            // Di chuyển viên đạn đến điểm trúng
            if (bulletVFX != null)
                StartCoroutine(MoveBulletVFX(bulletVFX, hit.point, hitTag));
        }
        else
        {
            if (bulletVFX != null)
                StartCoroutine(MoveBulletVFX(bulletVFX, ray.GetPoint(100f), hitTag)); // bay đến xa rồi tắt
        }
    }

    void HandleComboAttack()
    {
        if (Input.GetMouseButtonDown(1))
        {
            // Nếu chưa đánh gì thì bắt đầu Combo1
            if (!isAttacking)
            {
                comboStep = 1;
                requestedComboStep = 1;
                PlayComboAnimation(comboStep);
                isAttacking = true;
            }
            else
            {
                // Nếu đang đánh thì tăng combo mong muốn
                requestedComboStep = Mathf.Clamp(requestedComboStep + 1, 1, 3);
            }

            // Hiện gậy nếu đang ẩn
            if (staff != null && !staff.activeSelf)
                staff.SetActive(true);

            staffTimer = staffHideDelay;
        }

        // Ẩn gậy sau thời gian
        if (staff != null && staff.activeSelf)
        {
            staffTimer -= Time.deltaTime;
            if (staffTimer <= 0f)
                staff.SetActive(false);
        }
    }

    void PlayComboAnimation(int step)
    {
        if (animator != null)
        {
            animator.SetInteger("attackIndex", step);
            animator.SetTrigger("Attack");
            Debug.Log("Play Combo Step: " + step);
        }
    }

    public void MeleeAttackEnd()
    {
        // Kiểm tra có yêu cầu combo tiếp không
        if (requestedComboStep > comboStep && comboStep < 3)
        {
            comboStep++;
            PlayComboAnimation(comboStep);
        }
        else
        {
            // Kết thúc combo
            animator.ResetTrigger("Attack");
            animator.SetInteger("attackIndex", 0);
            animator.CrossFade("EllenIdle", 0.1f);

            isAttacking = false;
            comboStep = 0;
            requestedComboStep = 0;
        }
    }

    //effect cho từng combo
    public void SpawnComboVFX()
    {
        string comboTag = isBuffed ? $"BuffedCombo{comboStep}" : $"Combo{comboStep}";

        Transform spawnPoint = isBuffed && vfxSpawnPointProjectile != null
            ? vfxSpawnPointProjectile
            : (comboStep == 3 && vfxSpawnPointProjectile != null)
                ? vfxSpawnPointProjectile
                : vfxSpawnPoint;

        GameObject vfx = PoolingManager.Instance.SpawnFromPool(comboTag, spawnPoint.position, spawnPoint.rotation);

        if (vfx != null)
        {
            Rigidbody rb = vfx.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = spawnPoint.forward;
                float force = isBuffed ? buffVFXProjectileForce : projectileForce;
                rb.linearVelocity = Vector3.zero; // reset velocity
                rb.AddForce(direction * force, ForceMode.Impulse);
            }

            // Auto disable sau 2 giây
            StartCoroutine(DisableAfterSeconds(vfx, 2f));
        }

        Debug.Log("Spawn VFX Combo " + comboStep);

        if (comboStep == 1) PlaySound(combo1SFX);
        else if (comboStep == 2) PlaySound(combo2SFX);
        else if (comboStep == 3) PlaySound(combo3SFX);
    }

    private IEnumerator BuffRoutine()
    {
        isBuffed = true;

        // Tạo hiệu ứng quanh người từ pool
        if (buffAuraVFX != null)
        {
            currentAura = PoolingManager.Instance.SpawnFromPool("BuffAura", transform.position, Quaternion.identity);
            if (currentAura != null)
            {
                currentAura.transform.SetParent(transform);
            }
        }

        // Tăng damage vũ khí tay
        PlayerCombat.Instance?.ApplyDamageBuff(buffDamageAmount, buffDuration);

        // Tăng damage súng
        ApplyDamageBuff(buffDamageAmount, buffDuration);

        PlaySound(buffSFX);

        Debug.Log("BUFF ACTIVATED!");

        yield return new WaitForSeconds(buffDuration);

        isBuffed = false;

        // Tắt hiệu ứng buff sau khi hết buff
        if (currentAura != null)
        {
            currentAura.SetActive(false); // thay vì Destroy
            currentAura = null;
        }
    }

    private IEnumerator MoveBulletVFX(GameObject bullet, Vector3 target, string hitTag)
    {
        float speed = projectileVisualSpeed;

        while (bullet != null && bullet.activeSelf && Vector3.Distance(bullet.transform.position, target) > 0.1f)
        {
            bullet.transform.position = Vector3.MoveTowards(bullet.transform.position, target, speed * Time.deltaTime);
            yield return null;
        }

        if (bullet != null)
        {
            // tắt projectile
            bullet.SetActive(false);

            // spawn hit VFX một lần duy nhất
            GameObject hitVFX = PoolingManager.Instance.SpawnFromPool(hitTag, target, Quaternion.identity);
            if (hitVFX != null)
                StartCoroutine(DisableAfterSeconds(hitVFX, 1.5f));
        }
    }

    private IEnumerator DisableAfterSeconds(GameObject obj, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (obj != null && obj.activeSelf)
            obj.SetActive(false);
    }


    void ShootSkillV()
    {
        // Spawn từ pool thay vì Instantiate
        GameObject projectile =
            PoolingManager.Instance.SpawnFromPool("SkillProjectile", skillSpawnPoint.position,
                skillSpawnPoint.rotation);

        if (projectile != null)
        {
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.linearVelocity = skillSpawnPoint.forward * skillProjectileSpeed;

            SkillProjectile skillScript = projectile.GetComponent<SkillProjectile>();
            if (skillScript != null)
            {
                skillScript.burnEffectPrefab = burnEffectPrefab;
            }

            // Auto disable sau 5s (để tránh bay mãi)
            StartCoroutine(DisableAfterSeconds(projectile, 5f));
        }

        PlaySound(skillVSFX);
    }

    public void GainExp(int amount)
    {
        currentExp += amount;

        if (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        currentExp -= expToNextLevel;

        // Cứ mỗi level mới thì yêu cầu exp tăng thêm 50
        expToNextLevel += 50;

        // +20 HP max
        maxHP += 20;
        currentHP = maxHP;

        // Đồng bộ sang PlayerHealth
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.maxHealth = maxHP;
            health.Heal(maxHP); // full máu sau khi level up
        }

        if (_playerUI)
            _playerUI.UpdateHealth(currentHP, maxHP);

        // +10 dame cơ bản vĩnh viễn
        originalGunDamage += 10f;
        gunDamage = originalGunDamage;

        if (PlayerCombat.Instance != null)
        {
            PlayerCombat.Instance.AddPermanentDamage(10, 10);
        }

        Debug.Log(
            $"[LEVEL UP] Level {level} | Exp cần để lên level kế: {expToNextLevel} | HP: {maxHP} | GunDamage: {gunDamage}");
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void PlayFootstep(bool isSprinting)
    {
        if (audioSource == null) return;

        AudioClip[] clips = isSprinting ? sprintClips : footstepClips;
        if (clips == null || clips.Length == 0) return;

        int index = Random.Range(0, clips.Length);
        audioSource.PlayOneShot(clips[index]);
    }

    private void SavePlayerData()
    {
        DataManager.UpdateHealth(currentHP, maxHP);
        DataManager.UpdateExp(currentExp, level);
        DataManager.UpdatePosition(transform.position);
        DataManager.CurrentData.playerDamage = gunDamage;
        DataManager.CurrentData.moveSpeed = moveSpeed;
    }

    private void LoadPlayerDataToScene()
    {
        var data = DataManager.CurrentData;

        level = data.playerLevel;
        currentExp = data.playerExp;
        maxHP = data.playerMaxHP;
        currentHP = data.playerCurrentHP;
        gunDamage = data.playerDamage;
        moveSpeed = data.moveSpeed;

        Vector3 pos = new Vector3(data.positionX, data.positionY, data.positionZ);
        transform.position = pos;

        _playerUI.UpdateHealth(currentHP, maxHP);
    }

    private void OnApplicationQuit()
    {
        AutoSave("Thoát game");
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            AutoSave("Tạm dừng game (pause)");
    }

    private void OnDestroy()
    {
        if (gameObject.scene.isLoaded)
            AutoSave("Rời scene gameplay");
    }

    private void AutoSave(string reason)
    {
        SavePlayerData();
        DataManager.SaveGame();
        Debug.Log($"💾 [AutoSave] Game đã lưu ({reason})");
    }
}