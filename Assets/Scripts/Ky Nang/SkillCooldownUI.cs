using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUI : MonoBehaviour
{
    [Header("Hiển thị thời gian hồi chiêu")]
    public Text cooldownText;

    private float cooldownTime;
    private float cooldownTimer;
    private bool isCoolingDown = false;

    void Update()
    {
        if (isCoolingDown)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
            {
                cooldownTimer = 0f;
                isCoolingDown = false;
                cooldownText.text = ""; // Ẩn text khi hết hồi
            }
            else
            {
                // Làm tròn 1 chữ số sau dấu phẩy nếu muốn
                cooldownText.text = Mathf.Ceil(cooldownTimer).ToString();
            }
        }
    }

    public void StartCooldown(float duration)
    {
        cooldownTime = duration;
        cooldownTimer = duration;
        isCoolingDown = true;
        cooldownText.text = duration.ToString("0");
    }
}