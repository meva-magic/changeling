using UnityEngine;
using TMPro;
using System.Collections;

public class UIMessageManager : MonoBehaviour
{
    public static UIMessageManager Instance { get; private set; }
    
    [Header("UI References")]
    public GameObject messagePanel;
    public TextMeshProUGUI messageText;
    
    [Header("Settings")]
    public float defaultDisplayDuration = 3f;
    
    private Coroutine hideCoroutine;
    private bool isShowing = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (messagePanel != null)
            messagePanel.SetActive(false);
    }
    
    private void Update()
    {
        if (isShowing)
        {
            // Hide on space or mouse click
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                HideMessage();
            }
        }
    }
    
    public void ShowMessage(string messageKey, float duration = -1)
    {
        // Don't show if a minigame is active
        if (ClickerMinigameSystem.Instance != null && ClickerMinigameSystem.Instance.IsMinigameActive)
            return;
        
        string localizedMessage = GetLocalizedText(messageKey);
        
        if (messageText != null)
            messageText.text = localizedMessage;
        
        if (messagePanel != null)
            messagePanel.SetActive(true);
        
        isShowing = true;
        
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);
        
        float displayDuration = duration > 0 ? duration : defaultDisplayDuration;
        hideCoroutine = StartCoroutine(AutoHide(displayDuration));
    }
    
    public void HideMessage()
    {
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);
        
        if (messagePanel != null)
            messagePanel.SetActive(false);
        
        isShowing = false;
    }
    
    private IEnumerator AutoHide(float duration)
    {
        yield return new WaitForSeconds(duration);
        HideMessage();
    }
    
    private string GetLocalizedText(string key)
    {
        var table = UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase;
        if (table != null && !string.IsNullOrEmpty(key))
            return table.GetLocalizedString("UI Table", key);
        return key;
    }
    
    public bool IsShowing => isShowing;
}