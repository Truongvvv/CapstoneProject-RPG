using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuScene : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas _canvas;

    [Header("Buttons")]
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _settingButton;
    [SerializeField] private Button _helpButton;
    [SerializeField] private Button _exitButton;

    [Header("Dialogs")]
    [SerializeField] private GameObject _newGameDialog;
    [SerializeField] private GameObject _settingDialog;
    [SerializeField] private GameObject _helpDialog;

    public static Action<AudioType> PlaySound;

    private void Awake()
    {
        InitButtonEvents();

        _newGameDialog.gameObject.SetActive(false);
        _settingDialog.gameObject.SetActive(false);
        _helpDialog.gameObject.SetActive(false);

        if (!System.IO.File.Exists(Application.persistentDataPath + "/save.json"))
            _continueButton.interactable = false;
    }

    private void InitButtonEvents()
    {
        _continueButton.onClick.AddListener(OnContinueGameButtonPressed);
        _newGameButton.onClick.AddListener(OnNewGameButtonPressed);
        _settingButton.onClick.AddListener(OnSettingButtonPressed);
        _helpButton.onClick.AddListener(OnHelpButtonPressed);
        _exitButton.onClick.AddListener(OnExitButtonPressed);
    }

    #region OnClick Events

    private void OnExitButtonPressed()
    {
        PlaySound?.Invoke(AudioType.Click_01);
#if UNITY_EDITOR
        Debug.Log("Exit Game");
#else
        Application.Quit();
#endif
    }

    private void OnHelpButtonPressed()
    {
        PlaySound?.Invoke(AudioType.Click_01);
        _helpDialog.gameObject.SetActive(true);
    }

    private void OnSettingButtonPressed()
    {
        PlaySound?.Invoke(AudioType.Click_01);
        _settingDialog.gameObject.SetActive(true);
    }

    private void OnNewGameButtonPressed()
    {
        PlaySound?.Invoke(AudioType.Click_01);

        DataManager.DeleteSave();
        DataManager.CurrentData = new PlayerData();

        SceneManager.LoadScene(2);
    }

    private void OnContinueGameButtonPressed()
    {
        PlaySound?.Invoke(AudioType.Click_01);

        DataManager.LoadGame();

        if (DataManager.CurrentData != null)
            SceneManager.LoadScene(2);
        else
            Debug.LogWarning("⚠ Không có file save hợp lệ!");
    }

    #endregion
}
