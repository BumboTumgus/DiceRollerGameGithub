using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

public class OptionMenuController : MonoBehaviour
{
    private const string MASTER_AUDIO_FLOAT = "MasterVolume";
    private const string MUSIC_AUDIO_FLOAT = "MusicVolume";
    private const string EFFECTS_AUDIO_FLOAT = "EffectsVolume";
    private const string FULLSCREEN_INT = "FullscreenMode";
    private const string RESOLUTION_INT = "ResolutionSettings";
    private const float MUTE_DECIBELS = -80f;

    public UnityEvent PromptCancelAndDiscardChangesWindow;
    public UnityEvent StandardCancelNoChanges;

    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private Slider _musicVolumeSlider;
    [SerializeField] private Slider _effectsVolumeSlider;
    [SerializeField] private Toggle _fullScreenToggle;
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private GameObject[] _optionDivs;
    [SerializeField] private GameObject[] _optionSelectedHighlights;
    [SerializeField] private Resolution[] _availibleResolutions;


    private bool _optionsChanged = false;

    private void Start()
    {
        _resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach(Resolution resolution in _availibleResolutions)
            options.Add(resolution.horizontal + " x " + resolution.vertical);
        _resolutionDropdown.AddOptions(options);

        if(!PlayerPrefs.HasKey(MASTER_AUDIO_FLOAT))
        {
            PlayerPrefs.SetFloat(MASTER_AUDIO_FLOAT, 0);
            PlayerPrefs.SetFloat(MUSIC_AUDIO_FLOAT, 0);
            PlayerPrefs.SetFloat(EFFECTS_AUDIO_FLOAT, 0);
            PlayerPrefs.SetInt(FULLSCREEN_INT, 1);
            PlayerPrefs.SetInt(RESOLUTION_INT, 5);
        }
        else
            LoadOptionsFromPlayerPrefs();

        SetActiveDiv(0);
    }

    public void LoadOptionsFromPlayerPrefs()
    {
        _masterVolumeSlider.value = PlayerPrefs.GetFloat(MASTER_AUDIO_FLOAT);
        _musicVolumeSlider.value = PlayerPrefs.GetFloat(MUSIC_AUDIO_FLOAT);
        _effectsVolumeSlider.value = PlayerPrefs.GetFloat(EFFECTS_AUDIO_FLOAT);
        _fullScreenToggle.isOn = PlayerPrefs.GetInt(FULLSCREEN_INT) == 1 ? true : false;
        _resolutionDropdown.value = PlayerPrefs.GetInt(RESOLUTION_INT);

        SetMasterVolumeValue();
        SetEffectsVolumeValue();
        SetMusicVolumeValue();
        SetFullscreenValue();
        SetResolutionValue();

        _optionsChanged = false;
    }

    public void SaveOptionsToPlayerPrefs()
    {
        PlayerPrefs.SetFloat(MASTER_AUDIO_FLOAT, _masterVolumeSlider.value <= _masterVolumeSlider.minValue ? MUTE_DECIBELS : _masterVolumeSlider.value);
        PlayerPrefs.SetFloat(MUSIC_AUDIO_FLOAT, _musicVolumeSlider.value <= _musicVolumeSlider.minValue ? MUTE_DECIBELS : _musicVolumeSlider.value);
        PlayerPrefs.SetFloat(EFFECTS_AUDIO_FLOAT, _effectsVolumeSlider.value <= _effectsVolumeSlider.minValue ? MUTE_DECIBELS : _effectsVolumeSlider.value);
        PlayerPrefs.SetInt(FULLSCREEN_INT, _fullScreenToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt(RESOLUTION_INT, _resolutionDropdown.value);

        _optionsChanged = false;
    }

    public void CancelEditingOptions()
    {
        if (_optionsChanged)
            PromptCancelAndDiscardChangesWindow?.Invoke();
        else
            StandardCancelNoChanges?.Invoke();
    }

    public void SetMasterVolumeValue()
    {
        float value = _masterVolumeSlider.value;
        if (value <= _masterVolumeSlider.minValue)
            value = MUTE_DECIBELS;

        _audioMixer.SetFloat(MASTER_AUDIO_FLOAT, value);
        _optionsChanged = true;
    }

    public void SetMusicVolumeValue()
    {
        float value = _musicVolumeSlider.value;
        if (value <= _musicVolumeSlider.minValue)
            value = MUTE_DECIBELS;

        _audioMixer.SetFloat(MUSIC_AUDIO_FLOAT, value);
        _optionsChanged = true;
    }
    
    public void SetEffectsVolumeValue()
    {
        float value = _effectsVolumeSlider.value;
        if (value <= _effectsVolumeSlider.minValue)
            value = MUTE_DECIBELS;

        _audioMixer.SetFloat(EFFECTS_AUDIO_FLOAT, value);
        _optionsChanged = true;
    }

    public void SetFullscreenValue()
    {
        bool value = _fullScreenToggle.isOn;

        if (value)
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        else
            Screen.fullScreenMode = FullScreenMode.Windowed;

        Screen.SetResolution(_availibleResolutions[_resolutionDropdown.value].horizontal, _availibleResolutions[_resolutionDropdown.value].vertical, _fullScreenToggle.isOn);

        _optionsChanged = true;
    }

    public void SetResolutionValue()
    {
        int value = _resolutionDropdown.value;

        Screen.SetResolution(_availibleResolutions[value].horizontal, _availibleResolutions[value].vertical, _fullScreenToggle.isOn);

        _optionsChanged = true;
    }
    
    public void SetActiveDiv(int divIndex)
    {
        for(int index = 0; index < _optionDivs.Length; index++) 
        {
            if (index == divIndex)
            {
                _optionDivs[index].SetActive(true);
                _optionSelectedHighlights[index].SetActive(true);
            }
            else
            {
                _optionDivs[index].SetActive(false);
                _optionSelectedHighlights[index].SetActive(false);
            }
        }
    }

    [System.Serializable]
    public class Resolution
    {
        public int horizontal, vertical;
    }
}
