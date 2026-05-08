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
        Instance = this;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        Debug.Log("DialogueManager initialized");
    }
    
    public void ShowDialogue(SimpleDialogue dialogue, MonoBehaviour trigger)
    {
        Debug.Log($"ShowDialogue called with dialogue: {(dialogue != null ? dialogue.name : "null")}");
        
        currentDialogue = dialogue;
        currentTrigger = trigger;
        currentLineIndex = 0;
        isShowing = true;
        
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            ShowNextLine();
        }
        else
        {
            Debug.LogError("DialoguePanel is not assigned!");
        }
    }
    
    private void Update()
    {
        if (!isShowing) return;
        
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
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
            if (!string.IsNullOrEmpty(currentDialogue.voiceSoundName) && AudioManager.instance != null)
                AudioManager.instance.Play(currentDialogue.voiceSoundName);
            yield return new WaitForSeconds(textSpeed);
        }
        typingCoroutine = null;
    }
    
    private void EndDialogue()
    {
        Debug.Log("Dialogue ended");
        isShowing = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        
        if (currentTrigger != null)
        {
            var method = currentTrigger.GetType().GetMethod("OnDialogueFinished");
            method?.Invoke(currentTrigger, null);
        }
    }
}