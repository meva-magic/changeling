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
    [SerializeField] private bool blockPlayerInput = true;
    [SerializeField] private float autoCloseDelay = 3f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource letterAudioSource;
    
    private Coroutine typingCoroutine;
    private string currentFullText;
    private bool isTyping;
    private bool isVisible;
    private System.Action onCompleteCallback;
    private bool waitingForInput = false;
    private PlayerMovement playerMovement;
    private CursorController cursorController;
    private bool wasCursorLocked;
    private bool isDialogueActive = false;
    private bool shouldRestorePlayerControls = true;
    
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
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerMovement = player.GetComponent<PlayerMovement>();
        
        cursorController = ServiceLocator.Get<CursorController>();
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
    
    public void ShowDialogue(string messageKey, System.Action onComplete = null, bool restoreControls = true)
    {
        if (isDialogueActive) return;
        
        string localizedText = GetLocalizedText(messageKey);
        if (string.IsNullOrEmpty(localizedText)) return;
        
        isDialogueActive = true;
        shouldRestorePlayerControls = restoreControls;
        currentFullText = localizedText;
        onCompleteCallback = onComplete;
        
        dialoguePanel.SetActive(true);
        isVisible = true;
        waitingForInput = false;
        
        EventBus.Broadcast(GameEvents.MinigameStarted);
        
        if (blockPlayerInput && playerMovement != null)
        {
            playerMovement.SetMovementEnabled(false);
            if (cursorController != null)
            {
                wasCursorLocked = true;
                cursorController.UnlockForUI();
            }
        }
        
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
        
        if (autoCloseDelay > 0 && !blockPlayerInput)
        {
            yield return new WaitForSeconds(autoCloseDelay);
            CloseDialogue();
        }
        else
        {
            waitingForInput = true;
        }
    }
    
    private void SkipTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        dialogueText.text = currentFullText;
        isTyping = false;
        
        if (autoCloseDelay > 0 && !blockPlayerInput)
        {
            StartCoroutine(AutoCloseAfterDelay());
        }
        else
        {
            waitingForInput = true;
        }
    }
    
    private IEnumerator AutoCloseAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        CloseDialogue();
    }
    
    private void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        isVisible = false;
        isTyping = false;
        waitingForInput = false;
        isDialogueActive = false;
        
        EventBus.Broadcast(GameEvents.MinigameFinished);
        
        // Восстанавливаем управление только если нужно
        if (shouldRestorePlayerControls && blockPlayerInput && playerMovement != null)
        {
            playerMovement.SetMovementEnabled(true);
            if (cursorController != null && wasCursorLocked)
            {
                cursorController.LockForGameplay();
            }
        }
        
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        if (onCompleteCallback != null)
        {
            var callback = onCompleteCallback;
            onCompleteCallback = null;
            callback?.Invoke();
        }
    }
    
    public void ForceClose()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        dialoguePanel.SetActive(false);
        isVisible = false;
        isTyping = false;
        waitingForInput = false;
        isDialogueActive = false;
        
        EventBus.Broadcast(GameEvents.MinigameFinished);
        
        if (shouldRestorePlayerControls && blockPlayerInput && playerMovement != null)
        {
            playerMovement.SetMovementEnabled(true);
            if (cursorController != null && wasCursorLocked)
            {
                cursorController.LockForGameplay();
            }
        }
        
        onCompleteCallback = null;
    }
    
    public void SetBlockPlayerInput(bool block)
    {
        blockPlayerInput = block;
    }
    
    public void SetAutoCloseDelay(float delay)
    {
        autoCloseDelay = delay;
    }
    
    public bool IsVisible => isVisible;
    public bool IsDialogueActive => isDialogueActive;
    
    private string GetLocalizedText(string key)
    {
        return key;
    }
}