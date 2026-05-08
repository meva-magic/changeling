using UnityEngine;

public class SimpleDialogueTrigger : MonoBehaviour
{
    [SerializeField] private SimpleDialogue firstDialogue;
    [SerializeField] private SimpleDialogue reminderDialogue;
    [SerializeField] private SimpleDialogue questCompleteDialogue;
    [SerializeField] private SimpleDialogue postQuestDialogue;
    [SerializeField] private GameObject indicator;
    
    private bool playerInRange;
    private bool hasCompletedQuest;
    private SimpleDialogue currentDialogue;
    
    private void Start()
    {
        if (indicator != null) indicator.SetActive(false);
        currentDialogue = firstDialogue;
    }
    
    private void Update()
    {
        if (!playerInRange) return;
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
            SimpleDialogueManager.Instance.ShowDialogue(dialogueToShow, this);
    }
    
    private SimpleDialogue GetCurrentDialogue()
    {
        // Check if this NPC gave a quest that's active
        if (currentDialogue != null && currentDialogue.givesQuest && 
            SimpleQuestManager.Instance.IsQuestActive(currentDialogue.questID))
            return reminderDialogue;
        
        // Check if player has item to complete quest
        if (currentDialogue != null && currentDialogue.completesQuest &&
            SimpleQuestManager.Instance.IsQuestActive(currentDialogue.completeQuestID) &&
            SimpleQuestManager.Instance.CanCompleteQuest(currentDialogue.completeQuestID))
            return questCompleteDialogue;
        
        // Check if quest was already completed
        if (hasCompletedQuest && postQuestDialogue != null)
            return postQuestDialogue;
        
        return currentDialogue;
    }
    
    public void OnDialogueFinished()
    {
        if (currentDialogue != null)
        {
            if (currentDialogue.givesQuest && !string.IsNullOrEmpty(currentDialogue.questID))
            {
                SimpleQuestManager.Instance.StartQuest(currentDialogue.questID);
                indicator.SetActive(false);
            }
            
            if (currentDialogue.completesQuest && !string.IsNullOrEmpty(currentDialogue.completeQuestID))
            {
                SimpleQuestManager.Instance.CompleteQuest(currentDialogue.completeQuestID);
                hasCompletedQuest = true;
                currentDialogue = postQuestDialogue;
                indicator.SetActive(false);
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (indicator != null && currentDialogue != null)
                indicator.SetActive(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (indicator != null)
                indicator.SetActive(false);
        }
    }
}