using System;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
//using EditorAttributes;
using GameConfig;
using UnityEngine.SceneManagement;

public class MenuScene : MonoBehaviour
{
    [Header("UI References")]
    //[Required]
    [SerializeField]
    private Canvas _canvas;

    [Header("Buttons")] [Space(5)] [SerializeField]
    private Button _continueButton;

    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _settingButton;
    [SerializeField] private Button _helpButton;
    [SerializeField] private Button _exitButton;

    [Header("Dialogs")] [Space(5)] [SerializeField]
    private GameObject _newGameDialog;

    [SerializeField] private GameObject _settingDialog;
    [SerializeField] private GameObject _helpDialog;

    public static Action<AudioType> PlaySound;

    private void Awake()
    {
        InitButtonEvents();

        _newGameDialog.gameObject.SetActive(false);
        _settingDialog.gameObject.SetActive(false);
        _helpDialog.gameObject.SetActive(false);
    }

    private void InitButtonEvents()
    {
        _continueButton.onClick.AddListener(OnContinueGameButtonPressed);
        _newGameButton.onClick.AddListener(OnNewGameButtonPressed);
        _settingButton.onClick.AddListener(OnSettingButtonPressed);
        _helpButton.onClick.AddListener(OnHelpButtonPressed);
        _exitButton.onClick.AddListener(OnExitButtonPressed);
    }

    #region Onclick Events

    private void OnExitButtonPressed()
    {
        PlaySound?.Invoke(AudioType.Click_01);
#if UNITY_EDITOR
        Debug.Log("Exit");
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
        _newGameDialog.SetActive(true);
    }

    private void OnContinueGameButtonPressed()
    {
        PlaySound?.Invoke(AudioType.Click_01);
        SceneManager.LoadScene(2);
    }

    #endregion
}