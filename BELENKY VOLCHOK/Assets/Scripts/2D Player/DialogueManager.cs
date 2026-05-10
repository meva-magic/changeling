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
        if (dialogue == null)
        {
            Debug.LogError("Dialogue is null!");
            return;
        }
        
        int lineCount = dialogue.GetLineCount();
        if (lineCount == 0)
        {
            Debug.LogError($"Dialogue '{dialogue.name}' has NO lines!");
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
                if (currentDialogue != null && currentLineIndex < currentDialogue.GetLineCount())
                    dialogueText.text = currentDialogue.GetLine(currentLineIndex);
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
    }
    
    private void PlayVoiceSound()
    {
        if (currentDialogue == null) return;
        if (string.IsNullOrEmpty(currentDialogue.voiceSoundName)) return;
        if (AudioManager.instance == null) return;
        AudioManager.instance.Play(currentDialogue.voiceSoundName);
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