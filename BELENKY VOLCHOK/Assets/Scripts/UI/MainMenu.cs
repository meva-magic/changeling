using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    
    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmExitPanel;
    
    [Header("Confirm Exit Buttons")]
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;
    
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene";
    
    [Header("Audio")]
    [SerializeField] private string clickSoundName = "ui_click";
    
    private void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(StartGame);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
        
        if (exitButton != null)
            exitButton.onClick.AddListener(OpenConfirmExit);
        
        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(ConfirmExit);
        
        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(CloseConfirmExit);
        
        // Панель настроек и подтверждения скрыты при старте
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        if (confirmExitPanel != null)
            confirmExitPanel.SetActive(false);
        
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    private void StartGame()
    {
        PlayClickSound();
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
    }
    
    private void OpenSettings()
    {
        PlayClickSound();
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
        // Главное меню НЕ скрывается - настройки поверх него
    }
    
    private void OpenConfirmExit()
    {
        PlayClickSound();
        if (confirmExitPanel != null)
            confirmExitPanel.SetActive(true);
        // Главное меню НЕ скрывается - окно подтверждения поверх него
    }
    
    private void CloseConfirmExit()
    {
        PlayClickSound();
        if (confirmExitPanel != null)
            confirmExitPanel.SetActive(false);
    }
    
    private void ConfirmExit()
    {
        PlayClickSound();
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    private void PlayClickSound()
    {
        if (!string.IsNullOrEmpty(clickSoundName) && AudioManager.instance != null)
            AudioManager.instance.Play(clickSoundName);
    }
}