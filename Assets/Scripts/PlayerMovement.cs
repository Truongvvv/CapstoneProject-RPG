using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Di chuyển")]
    public float moveSpeed = 6f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    public float gunDamage = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Camera & Model")]
    public Transform cameraTransform;         // Gắn MainCamera
    public Transform modelTransform;          // Gắn object là model (có Animator)

    [Header("Combat")]
    public GameObject staff;                  // Gậy (hiện/ẩn)
    public float staffHideDelay = 5f;         // 5s không dùng thì ẩn
    private int comboStep = 0;         // 1 → 2 → 3
    private int requestedComboStep = 0; // combo player muốn (tăng theo lần nhấn)
    private bool isAttacking = false;
    private float staffTimer = 0f;            // Đếm ngược để ẩn gậy

    [Header("Combo VFX")]
    public GameObject[] comboVFX; // Gắn 3 hiệu ứng tương ứng combo 1, 2, 3
    public Transform vfxSpawnPointProjectile;  // Vị trí spawn VFX dạng bay

    [Header("VFX")]   
    public Transform vfxSpawnPoint;   // Gắn điểm đầu gậy (vị trí spawn)

    private CharacterController controller;
    private Animator animator;

    private Vector3 velocity;
    private bool isGrounded;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    public float sprintBonus = 5f; // Tăng thêm khi giữ Q

    [SerializeField] private Transform firePoint; // Gắn điểm bắn (nòng súng) trong Inspector

    [Header("Hiệu ứng")]
    public GameObject hitEffectPrefab; // Prefab hiệu ứng trúng đạn (tùy chọn)
    public Animator gunAnimator; // Gắn animator từ model/súng
    public float projectileForce = 500f; // Lực đẩy ra trước

    private float originalGunDamage; // Ghi nhớ damage gốc
    private float buffTimer = 0f;    // Đếm thời gian buff


    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = modelTransform.GetComponent<Animator>(); // Animator nằm trong model
        Cursor.lockState = CursorLockMode.Locked;
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
            // Chạy nhanh khi đè Q + có hướng di chuyển
            bool isSprinting = Input.GetKey(KeyCode.Q) && moveInput.magnitude > 0f;
            float currentSpeed = isSprinting ? moveSpeed + sprintBonus : moveSpeed;

            controller.Move(moveInput * currentSpeed * Time.deltaTime);
            //// Move thường
            //controller.Move(moveInput * moveSpeed * Time.deltaTime);
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
    }  

    void Shoot()
    {
        // Kích hoạt animation bắn
        if (gunAnimator != null)
        {
            gunAnimator.SetTrigger("Fire");
        }

        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint is not assigned!");
            return;
        }

        Ray ray = new Ray(firePoint.position, firePoint.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Debug.Log($"Raycast hit: {hit.collider.name}");

            // Gây damage nếu trúng enemy
            EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(gunDamage); // Gây 10 damage
                Debug.Log("Enemy bị bắn trúng! Gây 10 damage.");
            }

            // Spawn hiệu ứng trúng (nếu có)
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
        else
        {
            Debug.Log("Raycast missed");
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
        int index = Mathf.Clamp(comboStep - 1, 0, comboVFX.Length - 1);

        if (comboVFX.Length > index && comboVFX[index] != null)
        {
            Transform spawnPoint = comboStep == 3 && vfxSpawnPointProjectile != null
                ? vfxSpawnPointProjectile
                : vfxSpawnPoint;

            if (spawnPoint != null)
            {
                GameObject vfx = Instantiate(comboVFX[index], spawnPoint.position, spawnPoint.rotation);

                // Tự hủy sau 2 giây
                Destroy(vfx, 2f);

                if (comboStep == 3)
                {
                    Rigidbody rb = vfx.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddForce(spawnPoint.forward * projectileForce); // Bay ra trước
                    }
                    else
                    {
                        Debug.LogWarning("VFX combo 3 không có Rigidbody.");
                    }
                }

                Debug.Log("Spawn VFX Combo " + comboStep);
            }
        }
    }
}