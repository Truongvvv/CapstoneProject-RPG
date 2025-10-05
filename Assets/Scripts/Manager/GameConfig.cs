using UnityEngine;

namespace GameConfig
{
    /// <summary>
    /// Toàn bộ key dùng cho PlayerPrefs để lưu Game Settings.
    /// Giúp tránh bị sai chính tả khi đọc/ghi.
    /// </summary>
    public static class SettingKey
    {
        #region 🔊 Audio
        public const string MasterVolume = "master_volume";
        public const string MusicVolume = "music_volume";
        public const string SfxVolume = "sfx_volume";
        public const string VoiceVolume = "voice_volume";
        public const string MuteAll = "mute_all";
        #endregion

        #region 🎨 Graphics
        public const string Resolution = "resolution";
        public const string Fullscreen = "fullscreen";
        public const string GraphicQuality = "graphic_quality";
        public const string FpsLimit = "fps_limit";
        public const string TextureQuality = "texture_quality";
        public const string ShadowQuality = "shadow_quality";
        public const string AntiAliasing = "anti_aliasing";
        public const string PostProcessing = "post_processing";
        public const string RenderScale = "render_scale";
        #endregion

        #region 🎮 Controls
        public const string MouseSensitivity = "mouse_sensitivity";
        public const string InvertYAxis = "invert_y_axis";
        public const string InputDevice = "input_device";
        public const string Vibration = "vibration";
        #endregion

        #region ⚔️ Gameplay
        public const string Difficulty = "difficulty";
        public const string AutoSave = "auto_save";
        public const string Tutorial = "tutorial";
        public const string CameraShake = "camera_shake";
        public const string Hud = "hud";
        public const string FieldOfView = "fov";
        #endregion

        #region 🌍 Language & Region
        public const string Language = "language";
        public const string Subtitle = "subtitle";
        public const string Region = "region";
        #endregion

        #region 🌐 Online & Account
        public const string CrossPlatformPlay = "cross_platform_play";
        public const string VoiceChat = "voice_chat";
        public const string ChatFilter = "chat_filter";
        public const string ShowPing = "show_ping";
        public const string Privacy = "privacy";
        #endregion
    }
}
