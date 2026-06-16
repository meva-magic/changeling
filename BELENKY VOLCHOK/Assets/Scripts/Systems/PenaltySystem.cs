using UnityEngine;
using System.Collections;

public class PenaltySystem : MonoBehaviour
{
    public static PenaltySystem Instance { get; private set; }
    
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private UnityEngine.UI.Button replayButton;
    [SerializeField] private string currentSceneName;
    [SerializeField] private float flickerDuration = 0.2f;
    [SerializeField] private int flickerCount = 3;
    [SerializeField] private float finalFadeDuration = 1f;
    
    private CanvasGroup deathCanvasGroup;
    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
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
        if (replayButton != null) replayButton.onClick.AddListener(ReloadScene);
    }
    
    public void TriggerDeath()
    {
        Time.timeScale = 0f;
        AudioSource[] sources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in sources) source.Stop();
        StartCoroutine(DeathRoutine());
    }
    
    private IEnumerator DeathRoutine()
    {
        if (deathPanel != null) deathPanel.SetActive(true);
        deathCanvasGroup.alpha = 0f;
        for (int i = 0; i < flickerCount; i++)
        {
            deathCanvasGroup.alpha = 1f;
            yield return new WaitForSecondsRealtime(flickerDuration);
            deathCanvasGroup.alpha = 0f;
            yield return new WaitForSecondsRealtime(flickerDuration * 0.5f);
        }
        deathCanvasGroup.alpha = 1f;
        float elapsed = 0f;
        while (elapsed < finalFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            deathCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / finalFadeDuration);
            yield return null;
        }
        deathCanvasGroup.alpha = 0f;
        if (replayButton != null) replayButton.gameObject.SetActive(true);
    }
    
    private void ReloadScene()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
    }
}