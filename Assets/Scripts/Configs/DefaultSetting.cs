using UnityEngine;
using static GameSettingEnums; // để dùng lại enum trong GameSettingEnums.cs

/// <summary>
/// ScriptableObject lưu cấu hình mặc định cho game.
/// Có thể tạo nhiều profile khác nhau (ví dụ: Mobile, PC, High-End PC).
/// </summary>
[CreateAssetMenu(fileName = "DefaultSetting", menuName = "Configs/Settings/Default Setting")]
public class DefaultSetting : ScriptableObject
{
    [Header("🔊 Audio Settings")]
    [Range(0f, 1f)] public float masterInitVolume = 1.0f;
    [Range(0f, 1f)] public float musicInitVolume = 0.3f;
    [Range(0f, 1f)] public float sfxInitVolume = 0.3f;
    [Range(0f, 1f)] public float voiceInitVolume = 1.0f;
    public bool muteAll = false;

    [Header("🎨 Graphics Settings")]
    public ResolutionOption resolution = ResolutionOption.Res_1920x1080;
    public FullScreenSetting fullscreen = FullScreenSetting.Borderless;
    public GraphicQuality graphicQuality = GraphicQuality.High;
    public FpsLimit fpsLimit = FpsLimit.Fps60;
    public TextureQuality textureQuality = TextureQuality.High;
    public ShadowQuality shadowQuality = ShadowQuality.Medium;
    public AntiAliasing antiAliasing = AntiAliasing.FXAA;
    public bool postProcessing = true;
    [Range(50, 200)] public int renderScalePercent = 100;

    [Header("🎮 Control Settings")]
    [Range(0.1f, 10f)] public float mouseSensitivity = 1.5f;
    public InvertYAxis invertYAxis = InvertYAxis.Off;
    public InputDevice inputDevice = InputDevice.KeyboardMouse;
    public bool vibration = true;

    [Header("⚔️ Gameplay Settings")]
    public Difficulty difficulty = Difficulty.Normal;
    public bool autoSave = true;
    public bool tutorialEnabled = true;
    public bool cameraShake = true;
    public HudOption hud = HudOption.Full;
    [Range(60, 120)] public int fieldOfView = 90;

    [Header("🌍 Language & Region")]
    public Language language = Language.English;
    public bool subtitle = true;
    public Region region = Region.Auto;

    [Header("🌐 Online & Account Settings")]
    public CrossPlatformPlay crossPlatformPlay = CrossPlatformPlay.On;
    public VoiceChat voiceChat = VoiceChat.PushToTalk;
    public bool chatFilter = true;
    public bool showPing = true;
    public PrivacySetting privacy = PrivacySetting.Public;
}