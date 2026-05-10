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
    
    private void Start()
    {
        if (indicator != null) indicator.SetActive(false);
    }
    
    private void Update()
    {
        if (!playerInRange) return;
        if (SimpleDialogueManager.Instance == null) return;
        if (SimpleDialogueManager.Instance.IsShowing) return;
        if (SimpleDialogueManager.Instance.JustEnded) return; // Don't reopen immediately
        
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
        // CHECK IF PLAYER HAS QUEST ITEM
        if (firstDialogue != null && firstDialogue.completesQuest && 
            firstDialogue.questToComplete != null &&
            SimpleQuestManager.Instance != null &&
            SimpleQuestManager.Instance.IsQuestActive(firstDialogue.questToComplete) &&
            SimpleQuestManager.Instance.CanCompleteQuest(firstDialogue.questToComplete))
        {
            return questCompleteDialogue ?? firstDialogue;
        }
        
        // Quest was given and still active - show reminder
        if (questWasGiven && firstDialogue != null && firstDialogue.givesQuest && 
            firstDialogue.questToGive != null &&
            SimpleQuestManager.Instance != null &&
            SimpleQuestManager.Instance.IsQuestActive(firstDialogue.questToGive))
        {
            return reminderDialogue ?? firstDialogue;
        }
        
        // Completes quest + quest active (but player doesn't have item)
        if (firstDialogue != null && firstDialogue.completesQuest &&
            firstDialogue.questToComplete != null &&
            SimpleQuestManager.Instance != null &&
            SimpleQuestManager.Instance.IsQuestActive(firstDialogue.questToComplete))
        {
            return reminderDialogue ?? firstDialogue;
        }
        
        // Quest already completed - show post quest
        if (questWasCompleted && postQuestDialogue != null)
            return postQuestDialogue;
        
        // First dialogue was shown - show reminder
        if (firstDialogueShown && reminderDialogue != null)
            return reminderDialogue;
        
        // First time talking
        return firstDialogue;
    }
    
    public void OnDialogueFinished()
    {
        firstDialogueShown = true;
        
        if (firstDialogue == null) return;
        
        // GIVE QUEST
        if (firstDialogue.givesQuest && firstDialogue.questToGive != null)
        {
            if (SimpleQuestManager.Instance != null)
            {
                SimpleQuestManager.Instance.StartQuest(firstDialogue.questToGive);
                questWasGiven = true;
            }
        }
        
        // COMPLETE QUEST
        if (firstDialogue.completesQuest && firstDialogue.questToComplete != null)
        {
            if (SimpleQuestManager.Instance != null)
            {
                if (SimpleQuestManager.Instance.IsQuestActive(firstDialogue.questToComplete))
                {
                    SimpleQuestManager.Instance.CompleteQuest(firstDialogue.questToComplete);
                    questWasCompleted = true;
                }
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