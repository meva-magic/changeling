using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using System.Collections;

public class SettingsWindow : MonoBehaviour
{
    [Header("Volume")]
    [SerializeField] private Slider volumeSlider;
    
    [Header("Sound Toggle")]
    [SerializeField] private Button soundToggleButton;
    [SerializeField] private Image soundButtonImage;
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;
    
    [Header("Language")]
    [SerializeField] private Button englishButton;
    [SerializeField] private Button russianButton;
    
    [Header("Close")]
    [SerializeField] private Button closeButton;
    
    [Header("Audio")]
    [SerializeField] private string clickSoundName = "ui_click";
    
    private float currentVolume = 1f;
    private bool isSoundOn = true;
    
    private const string VOLUME_KEY = "MasterVolume";
    private const string SOUND_KEY = "SoundOn";
    private const string LANGUAGE_KEY = "Language";
    
    private void Start()
    {
        LoadSettings();
        ApplyVolume();
        UpdateSoundButtonIcon();
        
        if (volumeSlider != null)
        {
            volumeSlider.value = currentVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        
        if (soundToggleButton != null)
            soundToggleButton.onClick.AddListener(ToggleSound);
        
        if (englishButton != null)
            englishButton.onClick.AddListener(() => SetLanguage(0));
        
        if (russianButton != null)
            russianButton.onClick.AddListener(() => SetLanguage(1));
        
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseWindow);
    }
    
    private void LoadSettings()
    {
        currentVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
        isSoundOn = PlayerPrefs.GetInt(SOUND_KEY, 1) == 1;
        
        int savedLanguage = PlayerPrefs.GetInt(LANGUAGE_KEY, 0);
        StartCoroutine(SetLanguageAfterLoad(savedLanguage));
    }
    
    private void OnVolumeChanged(float value)
    {
        currentVolume = value;
        ApplyVolume();
        SaveSettings();
    }
    
    private void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        ApplyVolume();
        UpdateSoundButtonIcon();
        SaveSettings();
        PlayClickSound();
    }
    
    private void ApplyVolume()
    {
        float finalVolume = isSoundOn ? currentVolume : 0f;
        AudioListener.volume = finalVolume;
    }
    
    private void UpdateSoundButtonIcon()
    {
        if (soundButtonImage != null)
        {
            soundButtonImage.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
        }
    }
    
    private void SetLanguage(int localeIndex)
    {
        StartCoroutine(SetLanguageCoroutine(localeIndex));
        PlayClickSound();
    }
    
    private IEnumerator SetLanguageCoroutine(int localeIndex)
    {
        yield return LocalizationSettings.InitializationOperation;
        
        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (localeIndex >= 0 && localeIndex < locales.Count)
        {
            LocalizationSettings.SelectedLocale = locales[localeIndex];
            PlayerPrefs.SetInt(LANGUAGE_KEY, localeIndex);
            PlayerPrefs.Save();
        }
    }
    
    private IEnumerator SetLanguageAfterLoad(int localeIndex)
    {
        yield return LocalizationSettings.InitializationOperation;
        
        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (localeIndex >= 0 && localeIndex < locales.Count)
        {
            LocalizationSettings.SelectedLocale = locales[localeIndex];
        }
    }
    
    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(VOLUME_KEY, currentVolume);
        PlayerPrefs.SetInt(SOUND_KEY, isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    private void CloseWindow()
    {
        PlayClickSound();
        gameObject.SetActive(false); // Только скрываем настройки, главное меню остаётся
    }
    
    private void PlayClickSound()
    {
        if (!string.IsNullOrEmpty(clickSoundName) && AudioManager.instance != null)
            AudioManager.instance.Play(clickSoundName);
    }
}