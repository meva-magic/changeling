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
    private bool waitingForInput = false;
    
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
    
    private void Update()
    {
        if (!isVisible) return;
        
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                SkipTyping();
            }
            else if (waitingForInput)
            {
                CloseDialogue();
            }
        }
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
        waitingForInput = false;
        
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        typingCoroutine = StartCoroutine(TypeText());
    }
    
    private IEnumerator TypeText()
    {
        isTyping = true;
        dialogueText.text = "";
        
        foreach (char c in currentFullText)
        {
            dialogueText.text += c;
            
            if (!string.IsNullOrEmpty(letterSoundName))
                AudioManager.instance?.Play(letterSoundName);
            
            yield return new WaitForSeconds(letterDelay);
        }
        
        isTyping = false;
        waitingForInput = true;
    }
    
    private void SkipTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        dialogueText.text = currentFullText;
        isTyping = false;
        waitingForInput = true;
    }
    
    private void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        isVisible = false;
        isTyping = false;
        waitingForInput = false;
        
        onCompleteCallback?.Invoke();
    }
    
    public void ForceClose()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        dialoguePanel.SetActive(false);
        isVisible = false;
        isTyping = false;
        waitingForInput = false;
    }
    
    public bool IsVisible => isVisible;
    
    private string GetLocalizedText(string key)
    {
        return key;
    }
}