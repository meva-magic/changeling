using UnityEngine;

public class SimpleDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Assets")]
    [SerializeField] private SimpleDialogue firstDialogue;
    [SerializeField] private SimpleDialogue reminderDialogue;
    [SerializeField] private SimpleDialogue questCompleteDialogue;
    [SerializeField] private SimpleDialogue postQuestDialogue;
    
    [Header("Indicator")]
    [SerializeField] private GameObject indicator;
    
    private bool playerInRange;
    private bool questWasGiven;
    private bool questWasCompleted;
    private SimpleDialogue currentDialogue;
    private Collider2D npcCollider;
    
    private void Awake()
    {
        npcCollider = GetComponent<Collider2D>();
    }
    
    private void Start()
    {
        if (indicator != null) indicator.SetActive(false);
        currentDialogue = firstDialogue;
    }
    
    private void Update()
    {
        if (!playerInRange) return;
        if (SimpleDialogueManager.Instance == null) return;
        if (SimpleDialogueManager.Instance.IsShowing) return;
        
        // Check for interaction input
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            StartConversation();
        }
    }
    
    private void StartConversation()
    {
        SimpleDialogue dialogueToShow = GetCurrentDialogue();
        if (dialogueToShow != null && SimpleDialogueManager.Instance != null)
        {
            SimpleDialogueManager.Instance.ShowDialogue(dialogueToShow, this);
        }
    }
    
    private SimpleDialogue GetCurrentDialogue()
    {
        // If gave quest and quest is active - reminder
        if (questWasGiven && firstDialogue != null && firstDialogue.givesQuest && 
            SimpleQuestManager.Instance != null && 
            SimpleQuestManager.Instance.IsQuestActive(firstDialogue.questID))
        {
            if (reminderDialogue != null) return reminderDialogue;
        }
        
        // If completes quest and player has item - quest complete
        if (firstDialogue != null && firstDialogue.completesQuest &&
            SimpleQuestManager.Instance != null &&
            SimpleQuestManager.Instance.IsQuestActive(firstDialogue.completeQuestID) &&
            SimpleQuestManager.Instance.CanCompleteQuest(firstDialogue.completeQuestID))
        {
            if (questCompleteDialogue != null) return questCompleteDialogue;
        }
        
        // If quest completed - post quest
        if (questWasCompleted && postQuestDialogue != null)
            return postQuestDialogue;
        
        // First time
        return firstDialogue;
    }
    
    public void OnDialogueFinished()
    {
        if (firstDialogue != null)
        {
            // Check if this dialogue gives a quest
            if (firstDialogue.givesQuest && !string.IsNullOrEmpty(firstDialogue.questID))
            {
                if (SimpleQuestManager.Instance != null)
                {
                    SimpleQuestManager.Instance.StartQuest(firstDialogue.questID);
                    questWasGiven = true;
                }
            }
            
            // Check if this dialogue completes a quest
            if (firstDialogue.completesQuest && !string.IsNullOrEmpty(firstDialogue.completeQuestID))
            {
                if (SimpleQuestManager.Instance != null)
                {
                    SimpleQuestManager.Instance.CompleteQuest(firstDialogue.completeQuestID);
                    questWasCompleted = true;
                }
            }
        }
        
        // Show indicator again if there's more dialogue available
        UpdateIndicator();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            UpdateIndicator();
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (indicator != null) indicator.SetActive(false);
        }
    }
    
    private void UpdateIndicator()
    {
        if (indicator == null) return;
        
        bool hasDialogue = firstDialogue != null || 
                          (questWasGiven && reminderDialogue != null) ||
                          (questWasCompleted && postQuestDialogue != null);
        
        indicator.SetActive(playerInRange && hasDialogue);
    }
    
    // Public method so PlayerCarry can check if player is near this NPC
    public bool IsPlayerInRange()
    {
        return playerInRange;
    }
}