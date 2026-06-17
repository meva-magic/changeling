using UnityEngine;
using System.Collections;

public class PenaltySystem : MonoBehaviour
{
    public static PenaltySystem Instance { get; private set; }
    
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private UnityEngine.UI.Button replayButton;
    [SerializeField] private string currentSceneName;
    [SerializeField] private float fadeInDuration = 1f;
    
    private CanvasGroup deathCanvasGroup;
    private bool isDeathActive = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
            deathCanvasGroup = deathPanel.GetComponent<CanvasGroup>();
            if (deathCanvasGroup == null) deathCanvasGroup = deathPanel.AddComponent<CanvasGroup>();
            
            UnityEngine.UI.Image deathImage = deathPanel.GetComponent<UnityEngine.UI.Image>();
            if (deathImage == null) deathImage = deathPanel.AddComponent<UnityEngine.UI.Image>();
            deathImage.color = Color.black;
            deathCanvasGroup.alpha = 0f;
        }
        
        if (replayButton != null)
        {
            replayButton.onClick.AddListener(ReloadScene);
            replayButton.gameObject.SetActive(false);
        }
    }
    
    public void TriggerDeath()
    {
        if (isDeathActive) return;
        isDeathActive = true;
        
        Debug.Log("PenaltySystem: TriggerDeath вызван!");
        
        Time.timeScale = 0f;
        
        AudioSource[] sources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in sources)
        {
            source.Stop();
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        StartCoroutine(DeathRoutine());
    }
    
    private IEnumerator DeathRoutine()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
            deathCanvasGroup.alpha = 0f;
        }
        
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (deathCanvasGroup != null)
            {
                deathCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            }
            yield return null;
        }
        
        if (deathCanvasGroup != null)
        {
            deathCanvasGroup.alpha = 1f;
        }
        
        if (replayButton != null)
        {
            replayButton.gameObject.SetActive(true);
        }
        
        isDeathActive = false;
    }
    
    private void ReloadScene()
    {
        Debug.Log($"PenaltySystem: Перезагрузка сцены {currentSceneName}");
        
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (string.IsNullOrEmpty(currentSceneName))
        {
            Debug.LogError("PenaltySystem: currentSceneName не задан!");
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
            return;
        }
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
    }
    
    public void SetSceneName(string sceneName)
    {
        currentSceneName = sceneName;
    }
}