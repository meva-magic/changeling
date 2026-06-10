using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button menuButton;
    
    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmMenuPanel;
    
    [Header("Confirm Menu Buttons")]
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;
    
    [Header("Audio")]
    [SerializeField] private string clickSoundName = "ui_click";
    
    private bool isPaused;
    private PlayerStateManager playerState;
    
    private void Start()
    {
        playerState = FindObjectOfType<PlayerStateManager>();
        
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
        
        if (menuButton != null)
            menuButton.onClick.AddListener(OpenConfirmMenu);
        
        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(ConfirmGoToMenu);
        
        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(CloseConfirmMenu);
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        if (confirmMenuPanel != null)
            confirmMenuPanel.SetActive(false);
    }
    
    private void Update()
    {
        MinigameStarter minigame = ServiceLocator.Get<MinigameStarter>();
        if (minigame != null && minigame.IsMinigameActive) return;
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }
    
    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        if (playerState != null)
            playerState.SwitchToUI();
        
        gameObject.SetActive(true);
        PlayClickSound();
    }
    
    private void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (playerState != null)
            playerState.SwitchToGameplay();
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        if (confirmMenuPanel != null)
            confirmMenuPanel.SetActive(false);
        
        gameObject.SetActive(false);
        PlayClickSound();
    }
    
    private void OpenSettings()
    {
        PlayClickSound();
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }
    
    private void OpenConfirmMenu()
    {
        PlayClickSound();
        if (confirmMenuPanel != null)
            confirmMenuPanel.SetActive(true);
    }
    
    private void CloseConfirmMenu()
    {
        PlayClickSound();
        if (confirmMenuPanel != null)
            confirmMenuPanel.SetActive(false);
    }
    
    private void ConfirmGoToMenu()
    {
        PlayClickSound();
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    private void PlayClickSound()
    {
        if (!string.IsNullOrEmpty(clickSoundName) && AudioManager.instance != null)
            AudioManager.instance.Play(clickSoundName);
    }
}
