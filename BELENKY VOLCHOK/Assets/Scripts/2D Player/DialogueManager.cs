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
    
    public bool IsShowing => isShowing;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
    
    public void ShowDialogue(SimpleDialogue dialogue, MonoBehaviour trigger)
    {
        // Safety check
        if (dialogue == null)
        {
            Debug.LogError("Dialogue is null!");
            return;
        }
        
        if (dialogue.dialogueLines == null || dialogue.dialogueLines.Length == 0)
        {
            Debug.LogError($"Dialogue '{dialogue.name}' has NO lines! Add dialogue lines in the inspector.");
            return;
        }
        
        if (dialoguePanel == null)
        {
            Debug.LogError("Dialogue Panel not assigned!");
            return;
        }
        
        currentDialogue = dialogue;
        currentTrigger = trigger;
        currentLineIndex = 0;
        isShowing = true;
        
        dialoguePanel.SetActive(true);
        ShowNextLine();
    }
    
    private void Update()
    {
        if (!isShowing) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                if (currentLineIndex < currentDialogue.dialogueLines.Length)
                    dialogueText.text = currentDialogue.dialogueLines[currentLineIndex];
                typingCoroutine = null;
            }
            else
            {
                ShowNextLine();
            }
        }
    }
    
    private void ShowNextLine()
    {
        if (currentDialogue == null || currentDialogue.dialogueLines == null)
        {
            EndDialogue();
            return;
        }
        
        if (currentLineIndex < currentDialogue.dialogueLines.Length)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(currentDialogue.dialogueLines[currentLineIndex]));
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
            if (currentDialogue != null && !string.IsNullOrEmpty(currentDialogue.voiceSoundName))
            {
                if (AudioManager.instance != null)
                    AudioManager.instance.Play(currentDialogue.voiceSoundName);
            }
            yield return new WaitForSeconds(textSpeed);
        }
        typingCoroutine = null;
    }
    
    private void EndDialogue()
    {
        isShowing = false;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        if (currentTrigger != null)
        {
            var method = currentTrigger.GetType().GetMethod("OnDialogueFinished");
            method?.Invoke(currentTrigger, null);
        }
    }
}