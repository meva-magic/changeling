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
    private bool firstDialogueShown;
    private bool questWasGiven;
    private bool questWasCompleted;
    private string givenQuestID;
    private string completedQuestID;
    
    private void Start()
    {
        if (indicator != null) indicator.SetActive(false);
        
        // Store quest IDs for tracking
        if (firstDialogue != null && firstDialogue.givesQuest)
            givenQuestID = firstDialogue.questID;
        if (firstDialogue != null && firstDialogue.completesQuest)
            completedQuestID = firstDialogue.completeQuestID;
    }
    
    private void Update()
    {
        if (!playerInRange) return;
        if (SimpleDialogueManager.Instance == null) return;
        if (SimpleDialogueManager.Instance.IsShowing) return;
        
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            StartConversation();
        }
    }
    
    private void StartConversation()
    {
        SimpleDialogue dialogueToShow = GetCurrentDialogue();
        if (dialogueToShow != null)
        {
            SimpleDialogueManager.Instance.ShowDialogue(dialogueToShow, this);
        }
    }
    
    private SimpleDialogue GetCurrentDialogue()
    {
        // If this NPC completes a quest and player has the item
        if (firstDialogue != null && firstDialogue.completesQuest && 
            !string.IsNullOrEmpty(firstDialogue.completeQuestID) &&
            SimpleQuestManager.Instance != null &&
            SimpleQuestManager.Instance.IsQuestActive(firstDialogue.completeQuestID) &&
            SimpleQuestManager.Instance.CanCompleteQuest(firstDialogue.completeQuestID))
        {
            return questCompleteDialogue ?? firstDialogue;
        }
        
        // If this NPC gave a quest that's still active
        if (questWasGiven && !string.IsNullOrEmpty(givenQuestID) &&
            SimpleQuestManager.Instance != null &&
            SimpleQuestManager.Instance.IsQuestActive(givenQuestID))
        {
            return reminderDialogue ?? firstDialogue;
        }
        
        // If this NPC completes a quest and that quest is active (but player doesn't have item)
        if (firstDialogue != null && firstDialogue.completesQuest &&
            !string.IsNullOrEmpty(firstDialogue.completeQuestID) &&
            SimpleQuestManager.Instance != null &&
            SimpleQuestManager.Instance.IsQuestActive(firstDialogue.completeQuestID))
        {
            return reminderDialogue ?? firstDialogue;
        }
        
        // If quest was completed, show post-quest dialogue
        if (questWasCompleted && postQuestDialogue != null)
            return postQuestDialogue;
        
        // If first dialogue was already shown and we have a reminder, show reminder
        if (firstDialogueShown && reminderDialogue != null)
            return reminderDialogue;
        
        // First time - show first dialogue
        return firstDialogue;
    }
    
    public void OnDialogueFinished()
    {
        // Mark first dialogue as shown
        firstDialogueShown = true;
        
        if (firstDialogue == null) return;
        
        // Handle quest giving
        if (firstDialogue.givesQuest && !string.IsNullOrEmpty(firstDialogue.questID))
        {
            if (SimpleQuestManager.Instance != null)
            {
                SimpleQuestManager.Instance.StartQuest(firstDialogue.questID);
                questWasGiven = true;
                givenQuestID = firstDialogue.questID;
            }
        }
        
        // Handle quest completion
        if (firstDialogue.completesQuest && !string.IsNullOrEmpty(firstDialogue.completeQuestID))
        {
            if (SimpleQuestManager.Instance != null)
            {
                SimpleQuestManager.Instance.CompleteQuest(firstDialogue.completeQuestID);
                questWasCompleted = true;
                completedQuestID = firstDialogue.completeQuestID;
            }
        }
        
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
                          (firstDialogueShown && reminderDialogue != null) ||
                          (questWasCompleted && postQuestDialogue != null);
        
        indicator.SetActive(playerInRange && hasDialogue);
    }
    
    public bool IsPlayerInRange() { return playerInRange; }
}