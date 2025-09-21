using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameConfig; 

/// <summary>
/// Scene khởi động (Splash).
/// - Load DefaultSetting
/// - Khởi tạo PlayerPrefs lần đầu
/// - Chuyển sang MenuScene
/// </summary>
public class SplashScene : MonoBehaviour
{
    [SerializeField] private DefaultSetting _defaultSetting;

    private void Awake()
    {
        SetInitSetting();
        _ = LoadMenuScene();
    }

    /// <summary>
    /// Khởi tạo PlayerPrefs bằng DefaultSetting nếu chưa có dữ liệu.
    /// </summary>
    private void SetInitSetting()
    {
        // 🔊 Audio
        if (!PlayerPrefs.HasKey(SettingKey.MasterVolume))
            PlayerPrefs.SetFloat(SettingKey.MasterVolume, _defaultSetting.masterInitVolume);

        if (!PlayerPrefs.HasKey(SettingKey.MusicVolume))
            PlayerPrefs.SetFloat(SettingKey.MusicVolume, _defaultSetting.musicInitVolume);

        if (!PlayerPrefs.HasKey(SettingKey.SfxVolume))
            PlayerPrefs.SetFloat(SettingKey.SfxVolume, _defaultSetting.sfxInitVolume);

        if (!PlayerPrefs.HasKey(SettingKey.VoiceVolume))
            PlayerPrefs.SetFloat(SettingKey.VoiceVolume, _defaultSetting.voiceInitVolume);

        if (!PlayerPrefs.HasKey(SettingKey.MuteAll))
            PlayerPrefs.SetInt(SettingKey.MuteAll, _defaultSetting.muteAll ? 1 : 0);

        // 🎨 Graphics
        if (!PlayerPrefs.HasKey(SettingKey.Resolution))
            PlayerPrefs.SetInt(SettingKey.Resolution, (int)_defaultSetting.resolution);

        if (!PlayerPrefs.HasKey(SettingKey.Fullscreen))
            PlayerPrefs.SetInt(SettingKey.Fullscreen, (int)_defaultSetting.fullscreen);

        if (!PlayerPrefs.HasKey(SettingKey.GraphicQuality))
            PlayerPrefs.SetInt(SettingKey.GraphicQuality, (int)_defaultSetting.graphicQuality);

        if (!PlayerPrefs.HasKey(SettingKey.FpsLimit))
            PlayerPrefs.SetInt(SettingKey.FpsLimit, (int)_defaultSetting.fpsLimit);

        if (!PlayerPrefs.HasKey(SettingKey.TextureQuality))
            PlayerPrefs.SetInt(SettingKey.TextureQuality, (int)_defaultSetting.textureQuality);

        if (!PlayerPrefs.HasKey(SettingKey.ShadowQuality))
            PlayerPrefs.SetInt(SettingKey.ShadowQuality, (int)_defaultSetting.shadowQuality);

        if (!PlayerPrefs.HasKey(SettingKey.AntiAliasing))
            PlayerPrefs.SetInt(SettingKey.AntiAliasing, (int)_defaultSetting.antiAliasing);

        if (!PlayerPrefs.HasKey(SettingKey.PostProcessing))
            PlayerPrefs.SetInt(SettingKey.PostProcessing, _defaultSetting.postProcessing ? 1 : 0);

        if (!PlayerPrefs.HasKey(SettingKey.RenderScale))
            PlayerPrefs.SetInt(SettingKey.RenderScale, _defaultSetting.renderScalePercent);

        // 🎮 Controls
        if (!PlayerPrefs.HasKey(SettingKey.MouseSensitivity))
            PlayerPrefs.SetFloat(SettingKey.MouseSensitivity, _defaultSetting.mouseSensitivity);

        if (!PlayerPrefs.HasKey(SettingKey.InvertYAxis))
            PlayerPrefs.SetInt(SettingKey.InvertYAxis, (int)_defaultSetting.invertYAxis);

        if (!PlayerPrefs.HasKey(SettingKey.InputDevice))
            PlayerPrefs.SetInt(SettingKey.InputDevice, (int)_defaultSetting.inputDevice);

        if (!PlayerPrefs.HasKey(SettingKey.Vibration))
            PlayerPrefs.SetInt(SettingKey.Vibration, _defaultSetting.vibration ? 1 : 0);

        // ⚔️ Gameplay
        if (!PlayerPrefs.HasKey(SettingKey.Difficulty))
            PlayerPrefs.SetInt(SettingKey.Difficulty, (int)_defaultSetting.difficulty);

        if (!PlayerPrefs.HasKey(SettingKey.AutoSave))
            PlayerPrefs.SetInt(SettingKey.AutoSave, _defaultSetting.autoSave ? 1 : 0);

        if (!PlayerPrefs.HasKey(SettingKey.Tutorial))
            PlayerPrefs.SetInt(SettingKey.Tutorial, _defaultSetting.tutorialEnabled ? 1 : 0);

        if (!PlayerPrefs.HasKey(SettingKey.CameraShake))
            PlayerPrefs.SetInt(SettingKey.CameraShake, _defaultSetting.cameraShake ? 1 : 0);

        if (!PlayerPrefs.HasKey(SettingKey.Hud))
            PlayerPrefs.SetInt(SettingKey.Hud, (int)_defaultSetting.hud);

        if (!PlayerPrefs.HasKey(SettingKey.FieldOfView))
            PlayerPrefs.SetInt(SettingKey.FieldOfView, _defaultSetting.fieldOfView);

        // 🌍 Language & Region
        if (!PlayerPrefs.HasKey(SettingKey.Language))
            PlayerPrefs.SetInt(SettingKey.Language, (int)_defaultSetting.language);

        if (!PlayerPrefs.HasKey(SettingKey.Subtitle))
            PlayerPrefs.SetInt(SettingKey.Subtitle, _defaultSetting.subtitle ? 1 : 0);

        if (!PlayerPrefs.HasKey(SettingKey.Region))
            PlayerPrefs.SetInt(SettingKey.Region, (int)_defaultSetting.region);

        // 🌐 Online
        if (!PlayerPrefs.HasKey(SettingKey.CrossPlatformPlay))
            PlayerPrefs.SetInt(SettingKey.CrossPlatformPlay, (int)_defaultSetting.crossPlatformPlay);

        if (!PlayerPrefs.HasKey(SettingKey.VoiceChat))
            PlayerPrefs.SetInt(SettingKey.VoiceChat, (int)_defaultSetting.voiceChat);

        if (!PlayerPrefs.HasKey(SettingKey.ChatFilter))
            PlayerPrefs.SetInt(SettingKey.ChatFilter, _defaultSetting.chatFilter ? 1 : 0);

        if (!PlayerPrefs.HasKey(SettingKey.ShowPing))
            PlayerPrefs.SetInt(SettingKey.ShowPing, _defaultSetting.showPing ? 1 : 0);

        if (!PlayerPrefs.HasKey(SettingKey.Privacy))
            PlayerPrefs.SetInt(SettingKey.Privacy, (int)_defaultSetting.privacy);

        // Save toàn bộ
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load MenuScene sau khi SplashScene init xong.
    /// </summary>
    private async UniTask LoadMenuScene()
    {
        SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
        await UniTask.DelayFrame(1);
    }
}
