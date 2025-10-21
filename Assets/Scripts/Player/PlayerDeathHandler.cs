using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject playerModel;
    public MonoBehaviour[] controlScripts;

    [Header("UI Death Screen")]
    public GameObject deathUI;
    public Button respawnButton;
    public Button returnMenuButton;

    [Header("UI Player HUD (Thanh máu, mana, exp, v.v.)")]
    public GameObject playerHUD;

    [Header("Respawn Settings")]
    public Transform respawnPoint; // vị trí player hồi sinh

    private PlayerHealth playerHealth;
    private Camera mainCamera;

    private Button[] buttons;
    private int selectedIndex = 0;

    private void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (deathUI != null)
            deathUI.SetActive(false);

        mainCamera = Camera.main;
    }

    private void OnEnable() => PlayerHealth.OnPlayerDeath += OnPlayerDie;
    private void OnDisable() => PlayerHealth.OnPlayerDeath -= OnPlayerDie;

    private void Update()
    {
        if (deathUI == null || !deathUI.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            selectedIndex = (selectedIndex + 1) % buttons.Length;
            SelectButton(buttons[selectedIndex]);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            selectedIndex--;
            if (selectedIndex < 0) selectedIndex = buttons.Length - 1;
            SelectButton(buttons[selectedIndex]);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            buttons[selectedIndex].onClick.Invoke();
        }
    }

    private void OnPlayerDie()
    {
        // Tắt control player
        foreach (var s in controlScripts)
            if (s != null) s.enabled = false;

        // Ẩn HUD (UI máu, mana, v.v.)
        if (playerHUD != null)
            playerHUD.SetActive(false);

        // Hiện UI chết
        if (deathUI != null)
        {
            deathUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;

            buttons = new Button[] { respawnButton, returnMenuButton };
            selectedIndex = 0;
            SelectButton(buttons[selectedIndex]);
        }
    }

    public void Revive()
    {
        Time.timeScale = 1f;

        // Reset máu
        playerHealth.ResetHealth();

        // Hồi sinh tại spawn
        if (respawnPoint != null)
        {
            var controller = GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;

            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;

            if (controller != null) controller.enabled = true;
        }

        // Bật lại control
        foreach (var s in controlScripts)
            if (s != null) s.enabled = true;

        // Ẩn UI chết
        if (deathUI != null)
            deathUI.SetActive(false);

        // Bật lại toàn bộ HUD (UI máu, exp...)
        if (playerHUD != null)
            playerHUD.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SelectButton(Button button)
    {
        EventSystem.current.SetSelectedGameObject(button.gameObject);

        foreach (var b in buttons)
        {
            var colors = b.colors;
            colors.normalColor = (b == button) ? new Color(1f, 0.85f, 0.3f) : Color.gray;
            b.colors = colors;
        }
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Load scene menu chính
        SceneManager.LoadScene("MenuScene");
    }
}