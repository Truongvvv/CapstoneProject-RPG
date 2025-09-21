using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
//using EditorAttributes;
using GameConfig;

public class MenuScene : MonoBehaviour
{
    [Header("UI References")]
    //[Required]
    [SerializeField]
    private Canvas _canvas;

    [Header("Buttons")]
    [Space(5)]
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _settingButton;
    [SerializeField] private Button _helpButton;
    [SerializeField] private Button _exitButton;

    [Header("Dialogs")]
    [Space(5)]
    [SerializeField] private GameObject _newGameDialog;
    [SerializeField] private GameObject _settingDialog;
    [SerializeField] private GameObject _helpDialog;

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
#if UNITY_EDITOR
        Debug.Log("Exit");
#else
        Application.Quit();
#endif
    }

    private void OnHelpButtonPressed()
    {
        _helpDialog.gameObject.SetActive(true);
    }

    private void OnSettingButtonPressed()
    {
        _settingDialog.gameObject.SetActive(true);
    }

    private void OnNewGameButtonPressed()
    {
        _newGameDialog.SetActive(true);
    }

    private void OnContinueGameButtonPressed()
    {

    }
    #endregion
}
