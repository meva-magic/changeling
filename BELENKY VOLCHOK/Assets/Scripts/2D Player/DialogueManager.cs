using UnityEngine;
using TMPro;
using System.Collections;

public class SimpleDialogueManager : MonoBehaviour
{
    public static SimpleDialogueManager Instance;
    
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float textSpeed = 0.05f;
    
    private SimpleDialogue currentDialogue;
    private MonoBehaviour currentTrigger;
    private int currentLineIndex;
    private bool isShowing;
    private Coroutine typingCoroutine;
    private bool isLastLineFullyDisplayed;
    private float dialogueEndTime;
    private bool isReminder;
    private bool questPanelWasActive;
    
    public bool IsShowing => isShowing;
    public bool JustEnded => Time.time < dialogueEndTime + 0.3f;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
    
    public void ShowDialogue(SimpleDialogue dialogue, MonoBehaviour trigger)
    {
        if (dialogue == null) return;
        
        int lineCount = dialogue.GetLineCount();
        if (lineCount == 0) return;
        if (dialoguePanel == null) return;
        
        currentDialogue = dialogue;
        currentTrigger = trigger;
        currentLineIndex = 0;
        isShowing = true;
        isLastLineFullyDisplayed = false;
        isReminder = false;
        
        // Скрыть панель квеста если активна
        HideQuestPanel();
        
        dialoguePanel.SetActive(true);
        DisablePlayerMovement();
        
        ShowNextLine();
    }
    
    public void ShowReminder(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (dialoguePanel == null) return;
        if (isReminder) return;
        
        currentDialogue = null;
        currentTrigger = null;
        isShowing = true;
        isReminder = true;
        
        dialoguePanel.SetActive(true);
        dialogueText.text = text;
    }
    
    public void HideReminder()
    {
        if (!isReminder) return;
        
        isShowing = false;
        isReminder = false;
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
    
    private void HideQuestPanel()
    {
        if (SimpleQuestManager.Instance != null)
        {
            questPanelWasActive = SimpleQuestManager.Instance.IsQuestPanelActive();
            if (questPanelWasActive)
                SimpleQuestManager.Instance.HideQuestPanel();
        }
    }
    
    private void ShowQuestPanelIfWasActive()
    {
        if (questPanelWasActive && SimpleQuestManager.Instance != null)
        {
            SimpleQuestManager.Instance.ShowQuestPanel();
        }
    }
    
    private void Update()
    {
        if (!isShowing) return;
        if (isReminder) return;
        
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
                
                if (currentDialogue != null && currentLineIndex > 0 && 
                    currentLineIndex - 1 < currentDialogue.GetLineCount())
                {
                    dialogueText.text = currentDialogue.GetLine(currentLineIndex - 1);
                }
                
                if (currentDialogue != null && currentLineIndex >= currentDialogue.GetLineCount())
                    isLastLineFullyDisplayed = true;
            }
            else if (isLastLineFullyDisplayed)
            {
                EndDialogue();
            }
            else
            {
                ShowNextLine();
            }
        }
    }
    
    private void ShowNextLine()
    {
        if (currentDialogue == null)
        {
            EndDialogue();
            return;
        }
        
        int lineCount = currentDialogue.GetLineCount();
        
        if (currentLineIndex < lineCount)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            string line = currentDialogue.GetLine(currentLineIndex);
            typingCoroutine = StartCoroutine(TypeText(line));
            currentLineIndex++;
        }
        else
        {
            EndDialogue();
        }
    }
    
    private IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            PlayVoiceSound();
            yield return new WaitForSeconds(textSpeed);
        }
        typingCoroutine = null;
        
        if (currentDialogue != null && currentLineIndex >= currentDialogue.GetLineCount())
            isLastLineFullyDisplayed = true;
    }
    
    private void PlayVoiceSound()
    {
        if (currentDialogue == null) return;
        if (string.IsNullOrEmpty(currentDialogue.voiceSoundName)) return;
        if (AudioManager.instance == null) return;
        AudioManager.instance.Play(currentDialogue.voiceSoundName);
    }
    
    private void DisablePlayerMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
                movement.enabled = false;
        }
    }
    
    private void EnablePlayerMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
                movement.enabled = true;
        }
    }
    
    private void EndDialogue()
    {
        isShowing = false;
        isLastLineFullyDisplayed = false;
        dialogueEndTime = Time.time;
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        EnablePlayerMovement();
        
        // Показать панель квеста обратно если была активна
        ShowQuestPanelIfWasActive();
        
        if (currentTrigger != null)
        {
            var method = currentTrigger.GetType().GetMethod("OnDialogueFinished");
            method?.Invoke(currentTrigger, null);
        }
        
        currentDialogue = null;
        currentTrigger = null;
    }
}