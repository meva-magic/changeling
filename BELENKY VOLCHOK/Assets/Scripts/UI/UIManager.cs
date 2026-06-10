using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour, UserInterface
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject hintWindow;
    [SerializeField] private TextMeshProUGUI hintLabel;
    [SerializeField] private float defaultMessageDuration = 3f;
    
    private Coroutine activeHideCoroutine;
    private bool isMessageActive;
    
    private void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (hintWindow != null) hintWindow.SetActive(false);
    }
    
    public void ShowMessage(string messageKey, float duration = -1)
    {
        MinigameStarter minigame = ServiceLocator.Get<MinigameStarter>();
        if (minigame != null && minigame.IsMinigameActive) return;
        
        if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsVisible) return;
        
        string text = GetLocalizedText(messageKey);
        if (string.IsNullOrEmpty(text)) return;
        
        dialogueText.text = text;
        dialoguePanel.SetActive(true);
        isMessageActive = true;
        
        if (activeHideCoroutine != null) StopCoroutine(activeHideCoroutine);
        
        float displayTime = duration > 0 ? duration : defaultMessageDuration;
        activeHideCoroutine = StartCoroutine(HideAfterDelay(displayTime));
    }
    
    public void HideMessage()
    {
        if (activeHideCoroutine != null) StopCoroutine(activeHideCoroutine);
        dialoguePanel.SetActive(false);
        isMessageActive = false;
    }
    
    public void ShowHint(string hintKey)
    {
        string hintText = GetLocalizedText(hintKey);
        hintLabel.text = $"[{hintText}]";
        hintWindow.SetActive(true);
    }
    
    public void HideHint()
    {
        hintWindow.SetActive(false);
    }
    
    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideMessage();
    }
    
    private string GetLocalizedText(string key) => key;
    
    public bool IsMessageActive => isMessageActive;
}