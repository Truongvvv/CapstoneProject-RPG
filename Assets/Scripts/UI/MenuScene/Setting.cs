using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameConfig;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

public class SettingsUI : MonoBehaviour
{
    [Header("Audio UI")]
    [SerializeField] private Toggle _muteToggle;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("Graphics UI")]
    [SerializeField] private Toggle _fullscreenToggle;
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private Button _fps30Button;
    [SerializeField] private Button _fps60Button;
    [SerializeField] private Button _fps120Button;
    [SerializeField] private Button _fps144Button;
    [SerializeField] private Button _fpsUnlimitedButton;

    [SerializeField] private Button _closeButton;

    private Resolution[] _availableResolutions;
    private List<Button> _fpsButtons;
    private List<Image> _fpsButtonImages;

    [SerializeField] private Sprite _unselectedSprite;
    [SerializeField] private Sprite _selectedSprite;

    private void Awake()
    {
        _fpsButtons = new List<Button>
        {
            _fps30Button,
            _fps60Button,
            _fps120Button,
            _fps144Button,
            _fpsUnlimitedButton
        };

        _fpsButtonImages = new List<Image>();
        foreach (var button in _fpsButtons)
        {
            var buttonImage = button.GetComponent<Image>();
            _fpsButtonImages.Add(buttonImage);
        }

        InitEvent();

        // Apply FPS saved
        int savedFps = PlayerPrefs.GetInt(SettingKey.FpsLimit, 60);
        Application.targetFrameRate = savedFps;
        SetFps(savedFps);
    }

    private void InitEvent()
    {
        // 🔊 Init Audio
        _muteToggle.isOn = PlayerPrefs.GetInt(SettingKey.MuteAll, 0) == 1;
        _musicSlider.value = PlayerPrefs.GetFloat(SettingKey.MusicVolume, 0.7f);
        _sfxSlider.value = PlayerPrefs.GetFloat(SettingKey.SfxVolume, 0.8f);

        _muteToggle.onValueChanged.AddListener(OnMuteChanged);
        _musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        _sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

        // 🎨 Init Graphics
        _fullscreenToggle.isOn = PlayerPrefs.GetInt(SettingKey.Fullscreen, 1) == 1;
        _fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

        // Resolution
        _availableResolutions = Screen.resolutions;
        _resolutionDropdown.ClearOptions();
        int currentResIndex = 0;
        var options = new System.Collections.Generic.List<string>();

        for (int i = 0; i < _availableResolutions.Length; i++)
        {
            string option = $"{_availableResolutions[i].width} x {_availableResolutions[i].height}";
            options.Add(option);

            if (_availableResolutions[i].width == Screen.currentResolution.width &&
                _availableResolutions[i].height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        _resolutionDropdown.AddOptions(options);
        _resolutionDropdown.value = PlayerPrefs.GetInt(SettingKey.Resolution, currentResIndex);
        _resolutionDropdown.RefreshShownValue();
        _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        // FPS Buttons
        _fps30Button.onClick.AddListener(() => SetFps(30));
        _fps60Button.onClick.AddListener(() => SetFps(60));
        _fps120Button.onClick.AddListener(() => SetFps(120));
        _fps144Button.onClick.AddListener(() => SetFps(144));
        _fpsUnlimitedButton.onClick.AddListener(() => SetFps(-1));
        _closeButton.onClick.AddListener(ClosePanel);
    }

    #region 🔊 Audio
    private void OnMuteChanged(bool isOn)
    {
        PlayerPrefs.SetInt(SettingKey.MuteAll, isOn ? 1 : 0);
        PlayerPrefs.Save();
        AudioListener.volume = isOn ? 0 : 1;
    }

    private void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(SettingKey.MusicVolume, value);
        PlayerPrefs.Save();
    }

    private void OnSfxVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(SettingKey.SfxVolume, value);
        PlayerPrefs.Save();
    }
    #endregion

    #region 🎨 Graphics
    private void OnFullscreenChanged(bool isOn)
    {
        Screen.fullScreen = isOn;
        PlayerPrefs.SetInt(SettingKey.Fullscreen, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnResolutionChanged(int index)
    {
        var res = _availableResolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        PlayerPrefs.SetInt(SettingKey.Resolution, index);
        PlayerPrefs.Save();
    }

    private void SetFps(int fps)
    {
        Application.targetFrameRate = fps;
        PlayerPrefs.SetInt(SettingKey.FpsLimit, fps);
        PlayerPrefs.Save();

        HighlightFpsButtonByValue(fps);
    }

    private void HighlightFpsButton(Button buttonToSet)
    {
        for (int i = 0; i < _fpsButtons.Count; i++)
        {
            _fpsButtonImages[i].sprite = _unselectedSprite;
            _fpsButtons[i].transform.localScale = Vector3.one;
        }

        var idx = _fpsButtons.IndexOf(buttonToSet);
        if (idx >= 0)
        {
            _fpsButtonImages[idx].sprite = _selectedSprite;
            _fpsButtons[idx].transform.localScale = Vector3.one * 1.2f;
        }
    }

    private void HighlightFpsButtonByValue(int fps)
    {
        Button buttonToSet = null;

        if (fps == 30) buttonToSet = _fps30Button;
        else if (fps == 60) buttonToSet = _fps60Button;
        else if (fps == 120) buttonToSet = _fps120Button;
        else if (fps == 144) buttonToSet = _fps144Button;
        else if (fps <= 0) buttonToSet = _fpsUnlimitedButton; 

        if (buttonToSet != null)
            HighlightFpsButton(buttonToSet);
    }

    #endregion

    public void ClosePanel()
    {
        this.gameObject.SetActive(false);
    }
}
