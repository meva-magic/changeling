using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    
    [Header("Settings")]
    [SerializeField] private float letterDelay = 0.05f;
    [SerializeField] private string letterSoundName = "dialogue_letter";
    
    [Header("Audio")]
    [SerializeField] private AudioSource letterAudioSource;
    
    private Coroutine typingCoroutine;
    private string currentFullText;
    private bool isTyping;
    private bool isVisible;
    private System.Action onCompleteCallback;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
    
    public void ShowDialogue(string messageKey, System.Action onComplete = null)
    {
        if (isVisible) return;
        
        string localizedText = GetLocalizedText(messageKey);
        if (string.IsNullOrEmpty(localizedText)) return;
        
        currentFullText = localizedText;
        onCompleteCallback = onComplete;
        
        dialoguePanel.SetActive(true);
        isVisible = true;
        
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        typingCoroutine = StartCoroutine(TypeText());
    }
    
    public void SkipOrClose()
    {
        if (!isVisible) return;
        
        if (isTyping)
        {
            SkipTyping();
        }
        else
        {
            CloseDialogue();
        }
    }
    
    private void SkipTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        dialogueText.text = currentFullText;
        isTyping = false;
    }
    
    private void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        isVisible = false;
        isTyping = false;
        onCompleteCallback?.Invoke();
    }
    
    private IEnumerator TypeText()
    {
        isTyping = true;
        dialogueText.text = "";
        
        foreach (char c in currentFullText)
        {
            dialogueText.text += c;
            
            if (!string.IsNullOrEmpty(letterSoundName) && letterAudioSource != null)
            {
                AudioManager.instance?.Play(letterSoundName);
            }
            
            yield return new WaitForSeconds(letterDelay);
        }
        
        isTyping = false;
    }
    
    public bool IsVisible => isVisible;
    
    private string GetLocalizedText(string key) => key;
}