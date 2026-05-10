using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;
using System.Collections;

public class SettingsPanel : MonoBehaviour
{
    [Header("Audio UI")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button soundToggleButton;
    [SerializeField] private Image soundButtonImage;
    [SerializeField] private TextMeshProUGUI volumePercentText;
    
    [Header("Language UI")]
    [SerializeField] private Button englishButton;
    [SerializeField] private Button russianButton;
    
    [Header("Sprites")]
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;
    
    private float currentVolume = 1f;
    private bool isSoundOn = true;
    
    private const string VOLUME_KEY = "Volume";
    private const string SOUND_KEY = "SoundOn";
    private const string LANGUAGE_KEY = "Language";
    
    private void Start()
    {
        LoadSettings();
        FixInvalidVolume();
        
        // Audio listeners
        if (volumeSlider != null)
        {
            volumeSlider.value = currentVolume * 100f;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        
        if (soundToggleButton != null)
            soundToggleButton.onClick.AddListener(ToggleSound);
        
        // Language listeners - direct locale change
        if (englishButton != null)
            englishButton.onClick.AddListener(() => SetLanguage(0)); // English
        
        if (russianButton != null)
            russianButton.onClick.AddListener(() => SetLanguage(1)); // Russian
        
        UpdateUI();
        ApplyAudioSettings();
    }
    
    private void SetLanguage(int index)
    {
        StartCoroutine(SetLocale(index));
    }
    
    private IEnumerator SetLocale(int index)
    {
        // Wait for localization system to be ready
        yield return LocalizationSettings.InitializationOperation;
        
        // Get available locales
        var locales = LocalizationSettings.AvailableLocales.Locales;
        
        if (index < 0 || index >= locales.Count)
        {
            Debug.LogWarning($"Locale index {index} out of range. Available: {locales.Count}");
            yield break;
        }
        
        // Set the locale
        LocalizationSettings.SelectedLocale = locales[index];
        
        // Save preference
        PlayerPrefs.SetInt(LANGUAGE_KEY, index);
        PlayerPrefs.Save();
        
        Debug.Log($"Language changed to: {locales[index].name}");
    }
    
    private void FixInvalidVolume()
    {
        if (currentVolume > 1f || float.IsNaN(currentVolume) || currentVolume < 0f)
        {
            currentVolume = 1f;
            SaveSettings();
        }
    }
    
    private void OnVolumeChanged(float sliderValue)
    {
        currentVolume = sliderValue / 100f;
        UpdateVolumeDisplay();
        ApplyAudioSettings();
        SaveSettings();
    }
    
    private void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        UpdateUI();
        ApplyAudioSettings();
        SaveSettings();
    }
    
    private void ApplyAudioSettings()
    {
        AudioListener.volume = isSoundOn ? currentVolume : 0f;
    }
    
    private void UpdateUI()
    {
        if (volumeSlider != null)
            volumeSlider.interactable = isSoundOn;
        
        if (soundButtonImage != null)
            soundButtonImage.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
        
        UpdateVolumeDisplay();
    }
    
    private void UpdateVolumeDisplay()
    {
        if (volumePercentText != null)
        {
            int percent = Mathf.RoundToInt(currentVolume * 100);
            volumePercentText.text = $"{percent}%";
        }
    }
    
    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(VOLUME_KEY, currentVolume);
        PlayerPrefs.SetInt(SOUND_KEY, isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    private void LoadSettings()
    {
        currentVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
        isSoundOn = PlayerPrefs.GetInt(SOUND_KEY, 1) == 1;
        
        // Load saved language
        int savedLanguage = PlayerPrefs.GetInt(LANGUAGE_KEY, 0);
        StartCoroutine(LoadLocale(savedLanguage));
    }
    
    private IEnumerator LoadLocale(int index)
    {
        yield return LocalizationSettings.InitializationOperation;
        
        var locales = LocalizationSettings.AvailableLocales.Locales;
        
        if (index >= 0 && index < locales.Count)
        {
            LocalizationSettings.SelectedLocale = locales[index];
        }
    }
}