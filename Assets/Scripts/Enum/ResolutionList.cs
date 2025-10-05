using UnityEngine;

/// <summary>
/// Chứa toàn bộ enum cho Game Settings.
/// Dùng để hiển thị và lưu trữ các tùy chọn setting phổ biến.
/// </summary>
public static class GameSettingEnums { }

#region Graphics

/// <summary>
/// Các độ phân giải phổ biến (Width x Height).
/// Dùng extension để convert sang Vector2Int.
/// </summary>
public enum ResolutionOption
{
    None = 0,
    Res_1280x720,
    Res_1600x900,
    Res_1920x1080,
    Res_2560x1440,
    Res_3840x2160,
    Res_5120x2880,
    Res_7680x4320,
}

/// <summary>
/// Chế độ hiển thị màn hình.
/// </summary>
public enum FullScreenSetting
{
    None = 0,
    Fullscreen,
    Borderless,
    Windowed
}

/// <summary>
/// Chất lượng đồ họa tổng thể.
/// </summary>
public enum GraphicQuality
{
    None = 0,
    Low,
    Medium,
    High,
    Ultra
}

/// <summary>
/// Giới hạn FPS.
/// Có thể gán trực tiếp vào Application.targetFrameRate.
/// </summary>
public enum FpsLimit
{
    None = 0,
    Fps45 = 45,
    Fps60 = 60,
    Fps120 = 120,
    Fps144 = 144,
    Unlimited = -1
}

/// <summary>
/// Chất lượng texture.
/// </summary>
public enum TextureQuality
{
    Low,
    Medium,
    High,
    Ultra
}

/// <summary>
/// Chất lượng bóng đổ.
/// </summary>
public enum ShadowQuality
{
    Off,
    Low,
    Medium,
    High,
    Ultra
}

/// <summary>
/// Các kỹ thuật Anti-Aliasing.
/// </summary>
public enum AntiAliasing
{
    Off,
    FXAA,
    SMAA,
    TAA,
    MSAA2x,
    MSAA4x,
    MSAA8x
}

#endregion

#region Audio

/// <summary>
/// Các loại âm thanh có thể chỉnh riêng.
/// </summary>
public enum AudioChannel
{
    Master,
    Music,
    SFX,
    Voice
}

#endregion

#region Controls

/// <summary>
/// Tùy chọn đảo chiều trục Y cho camera.
/// </summary>
public enum InvertYAxis
{
    Off,
    On
}

/// <summary>
/// Các loại input chính.
/// </summary>
public enum InputDevice
{
    KeyboardMouse,
    Gamepad,
    Touch
}

#endregion

#region Gameplay

/// <summary>
/// Mức độ khó của game.
/// </summary>
public enum Difficulty
{
    Easy,
    Normal,
    Hard,
    VeryHard,
    Nightmare
}

/// <summary>
/// Tùy chọn hiển thị HUD.
/// </summary>
public enum HudOption
{
    Full,
    Minimal,
    Custom,
    Hidden
}

#endregion

#region Language & Region

/// <summary>
/// Ngôn ngữ hỗ trợ.
/// </summary>
public enum Language
{
    English,
    Vietnamese,
    Japanese,
    ChineseSimplified,
    ChineseTraditional,
    Korean,
    Spanish,
    French,
    German,
    Russian,
    Portuguese,
    Arabic
}

/// <summary>
/// Server / Khu vực kết nối.
/// </summary>
public enum Region
{
    Auto,
    Asia,
    NorthAmerica,
    Europe,
    SouthAmerica,
    Oceania
}

#endregion

#region Online & Account

/// <summary>
/// Trạng thái cross-platform.
/// </summary>
public enum CrossPlatformPlay
{
    Off,
    On
}

/// <summary>
/// Tùy chọn voice chat.
/// </summary>
public enum VoiceChat
{
    Disabled,
    Enabled,
    PushToTalk
}

/// <summary>
/// Quyền riêng tư khi chơi online.
/// </summary>
public enum PrivacySetting
{
    Public,
    FriendsOnly,
    Private
}

#endregion

// =============================
// 📌 Extension hỗ trợ
// =============================
public static class ResolutionExtensions
{
    public static Vector2Int ToSize(this ResolutionOption resolution)
    {
        return resolution switch
        {
            ResolutionOption.Res_1280x720 => new Vector2Int(1280, 720),
            ResolutionOption.Res_1600x900 => new Vector2Int(1600, 900),
            ResolutionOption.Res_1920x1080 => new Vector2Int(1920, 1080),
            ResolutionOption.Res_2560x1440 => new Vector2Int(2560, 1440),
            ResolutionOption.Res_3840x2160 => new Vector2Int(3840, 2160),
            ResolutionOption.Res_5120x2880 => new Vector2Int(5120, 2880),
            ResolutionOption.Res_7680x4320 => new Vector2Int(7680, 4320),
            _ => Vector2Int.zero,
        };
    }
}
