using UnityEngine;

public class SimpleDialogueTrigger : MonoBehaviour
{
    [SerializeField] private SimpleDialogue firstDialogue;
    [SerializeField] private SimpleDialogue reminderDialogue;
    [SerializeField] private SimpleDialogue questCompleteDialogue;
    [SerializeField] private SimpleDialogue postQuestDialogue;
    [SerializeField] private GameObject indicator;
    
    private bool playerInRange;
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
        if (questWasGiven && firstDialogue != null && firstDialogue.givesQuest)
        {
            if (SimpleQuestManager.Instance != null && 
                SimpleQuestManager.Instance.IsQuestActive(firstDialogue.questID))
                return reminderDialogue;
        }
        
        if (firstDialogue != null && firstDialogue.completesQuest)
        {
            if (SimpleQuestManager.Instance != null && 
                SimpleQuestManager.Instance.IsQuestActive(firstDialogue.completeQuestID) &&
                SimpleQuestManager.Instance.CanCompleteQuest(firstDialogue.completeQuestID))
                return questCompleteDialogue;
        }
        
        if (questWasCompleted && postQuestDialogue != null)
            return postQuestDialogue;
        
        return firstDialogue;
    }
    
    public void OnDialogueFinished()
    {
        if (firstDialogue != null)
        {
            if (firstDialogue.givesQuest && !string.IsNullOrEmpty(firstDialogue.questID))
            {
                if (SimpleQuestManager.Instance != null)
                {
                    SimpleQuestManager.Instance.StartQuest(firstDialogue.questID);
                    questWasGiven = true;
                }
            }
            
            if (firstDialogue.completesQuest && !string.IsNullOrEmpty(firstDialogue.completeQuestID))
            {
                if (SimpleQuestManager.Instance != null)
                {
                    SimpleQuestManager.Instance.CompleteQuest(firstDialogue.completeQuestID);
                    questWasCompleted = true;
                }
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        
        playerInRange = true;
        
        if (indicator != null)
            indicator.SetActive(true);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        
        playerInRange = false;
        
        if (indicator != null)
            indicator.SetActive(false);
    }
}