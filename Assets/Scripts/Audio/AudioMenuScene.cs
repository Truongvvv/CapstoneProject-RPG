using System;
using UnityEngine;
using GameConfig;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public enum AudioType
{
    Null,
    Background,
    Click_01,
    Click_02,
}

public class AudioMenuScene : MonoBehaviour
{
    [SerializeField] private AudioSource _musicAudioManager;
    [SerializeField] private AudioSource _sfxAudioManager;

    [SerializeField] private AudioClip _backgroundMusic;
    [SerializeField] private AudioClip _click_01Music;
    [SerializeField] private AudioClip _click_02Music;

    private void Start()
    {
        float musicVolume = PlayerPrefs.GetFloat(GameConfig.SettingKey.MusicVolume, 1.0f);
        float sfxVolume = PlayerPrefs.GetFloat(GameConfig.SettingKey.SfxVolume, 1.0f);

        _musicAudioManager.clip = _backgroundMusic;
        _musicAudioManager.loop = true;
        _musicAudioManager.volume = musicVolume;
        _musicAudioManager.Play();

        _sfxAudioManager.volume = sfxVolume;
    }

    private void OnEnable()
    {
        SettingsUI.OnMusicVolumeChange += HandleMusicVolumeChanged;
        SettingsUI.OnSfxVolumeChange += HandleSfxVolumeChanged;
        MenuScene.PlaySound += PlaySound;
        SettingsUI.PlaySound += PlaySound;
    }

    private void OnDisable()
    {
        SettingsUI.OnMusicVolumeChange -= HandleMusicVolumeChanged;
        SettingsUI.OnSfxVolumeChange -= HandleSfxVolumeChanged;
        MenuScene.PlaySound -= PlaySound;
        SettingsUI.PlaySound -= PlaySound;
    }

    private void HandleMusicVolumeChanged(float value)
    {
        _musicAudioManager.volume = value;
    }

    private void HandleSfxVolumeChanged(float value)
    {
        _sfxAudioManager.volume = value;
    }

    public void PlaySound(AudioType type)
    {
        switch (type)
        {
            case AudioType.Click_01:
                _sfxAudioManager.PlayOneShot(_click_01Music);
                break;
            case AudioType.Click_02:
                _sfxAudioManager.PlayOneShot(_click_02Music);
                break;
        }
    }
}