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
    private float dialogueEndTime; // Cooldown to prevent immediate reopen
    
    public bool IsShowing => isShowing;
    public bool JustEnded => Time.time < dialogueEndTime + 0.3f; // 0.3 second cooldown
    
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
        
        dialoguePanel.SetActive(true);
        DisablePlayerMovement();
        
        ShowNextLine();
    }
    
    private void Update()
    {
        if (!isShowing) return;
        
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (typingCoroutine != null)
            {
                // Still typing - skip to end of current line
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
                
                // Show full line immediately
                if (currentDialogue != null && currentLineIndex > 0 && 
                    currentLineIndex - 1 < currentDialogue.GetLineCount())
                {
                    dialogueText.text = currentDialogue.GetLine(currentLineIndex - 1);
                }
                
                // If this was the last line, mark it
                if (currentDialogue != null && currentLineIndex >= currentDialogue.GetLineCount())
                {
                    isLastLineFullyDisplayed = true;
                }
            }
            else if (isLastLineFullyDisplayed)
            {
                // Last line is fully displayed - close dialogue
                EndDialogue();
            }
            else
            {
                // Go to next line
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
        
        // Check if this was the last line
        if (currentDialogue != null && currentLineIndex >= currentDialogue.GetLineCount())
        {
            isLastLineFullyDisplayed = true;
        }
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
        dialogueEndTime = Time.time; // Set cooldown time
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        EnablePlayerMovement();
        
        if (currentTrigger != null)
        {
            var method = currentTrigger.GetType().GetMethod("OnDialogueFinished");
            method?.Invoke(currentTrigger, null);
        }
        
        // Clear references
        currentDialogue = null;
        currentTrigger = null;
    }
}