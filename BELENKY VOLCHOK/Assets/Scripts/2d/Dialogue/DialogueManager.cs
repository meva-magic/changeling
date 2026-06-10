using System;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private float textSpeed = 0.05f;

    private DialogueNode currentNode;
    private int currentLineIndex;
    private bool isShowing;
    private Coroutine typingCoroutine;
    private bool isLineFullyDisplayed;
    private bool isReminder;
    private bool ignoreNextInput;

    public event Action OnDialogueStarted;
    public event Action OnDialogueEnded;

    public bool IsShowing => isShowing;
    public bool JustEnded { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        HideDialogue();
    }

    private void Update()
    {
        JustEnded = false;

        if (!isShowing) return;
        if (isReminder) return;

        if (ignoreNextInput)
        {
            ignoreNextInput = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (!isLineFullyDisplayed)
            {
                SkipTyping();
            }
            else
            {
                AdvanceDialogue();
            }
        }
    }

    private void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (currentNode != null)
            bodyText.text = currentNode.GetLine(currentLineIndex);

        isLineFullyDisplayed = true;
    }

    public void ShowDialogue(DialogueNode node)
    {
        if (node == null) return;

        currentNode = node;
        currentLineIndex = 0;
        isShowing = true;
        isLineFullyDisplayed = false;
        isReminder = false;
        JustEnded = false;
        ignoreNextInput = true;

        BlockPlayerMovement(true);

        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        DisplayCurrentLine();

        OnDialogueStarted?.Invoke();

        if (!string.IsNullOrEmpty(node.voiceSound) && AudioManager.instance != null)
            AudioManager.instance.Play(node.voiceSound);
    }

    private void BlockPlayerMovement(bool block)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
                controller.InputBlocked = block;
        }
    }

    private void DisplayCurrentLine()
    {
        if (currentNode == null) return;

        string line = currentNode.GetLine(currentLineIndex);
        if (string.IsNullOrEmpty(line))
        {
            isLineFullyDisplayed = true;
            bodyText.text = "";
            return;
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isLineFullyDisplayed = false;
        typingCoroutine = StartCoroutine(TypeText(line));
    }

    private System.Collections.IEnumerator TypeText(string text)
    {
        bodyText.text = "";
        yield return null;

        foreach (char c in text)
        {
            bodyText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isLineFullyDisplayed = true;
        typingCoroutine = null;
    }

    private void AdvanceDialogue()
    {
        if (currentNode == null)
        {
            HideDialogue();
            return;
        }

        currentLineIndex++;

        if (currentLineIndex < currentNode.GetLineCount())
        {
            DisplayCurrentLine();
        }
        else
        {
            ProcessNodeEnd();
        }
    }

    private void ProcessNodeEnd()
    {
        if (currentNode.responses != null && currentNode.responses.Count > 0)
        {
            bool questJustCompleted = false;

            foreach (DialogueResponse response in currentNode.responses)
            {
                ProcessResponse(response);
                
                if (response.completesQuest && response.questToComplete != null)
                    questJustCompleted = true;
            }

            if (questJustCompleted)
            {
                ScriptedEvent scriptedEvent = FindObjectOfType<ScriptedEvent>();
                if (scriptedEvent != null)
                    scriptedEvent.SwapBabyForChangeling();
            }

            if (currentNode.responses[0].nextNode != null)
            {
                ShowDialogue(currentNode.responses[0].nextNode);
                return;
            }
        }

        HideDialogue();
    }

    private void ProcessResponse(DialogueResponse response)
    {
        if (response.givesQuest && response.questToGive != null)
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.StartQuest(response.questToGive);
        }

        if (response.completesQuest && response.questToComplete != null)
        {
            if (QuestManager.Instance != null)
            {
                DestroyQuestItem(response.questToComplete.requiredItemID);
                QuestManager.Instance.CompleteQuest(response.questToComplete);
            }
        }
    }

    private void DestroyQuestItem(string itemID)
    {
        PlayerCarry playerCarry = FindObjectOfType<PlayerCarry>();
        if (playerCarry != null && playerCarry.IsCarryingObject)
        {
            PickupableItem item = playerCarry.CarriedObject?.GetComponent<PickupableItem>();
            if (item != null && item.itemID == itemID)
                Destroy(playerCarry.CarriedObject);
        }
    }

    public void ShowReminder(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (dialoguePanel == null) return;
        if (isReminder) return;

        currentNode = null;
        isShowing = true;
        isReminder = true;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (bodyText != null) bodyText.text = text;
    }

    public void HideReminder()
    {
        if (!isReminder) return;

        isShowing = false;
        isReminder = false;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    public void HideDialogue()
    {
        isShowing = false;

        BlockPlayerMovement(false);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        OnDialogueEnded?.Invoke();
    }

    public DialogueNode GetLastNode() => currentNode;
}