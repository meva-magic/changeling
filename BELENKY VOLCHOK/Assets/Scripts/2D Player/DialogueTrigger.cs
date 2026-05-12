using UnityEngine;
using UnityEngine.Events;

public class SimpleDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Assets")]
    [SerializeField] private SimpleDialogue firstDialogue;
    [SerializeField] private SimpleDialogue reminderDialogue;
    [SerializeField] private SimpleDialogue questCompleteDialogue;
    [SerializeField] private SimpleDialogue postQuestDialogue;
    
    [Header("Indicator")]
    [SerializeField] private GameObject indicator;
    
    [Header("Events")]
    [SerializeField] private UnityEvent onQuestStarted;
    [SerializeField] private UnityEvent onQuestCompleted;
    
    private bool playerInRange;
    private bool firstDialogueShown;
    private bool questWasGiven;
    private bool questWasCompleted;
    private SimpleQuest activeQuestForThisNPC;
    
    private void Start()
    {
        if (indicator != null) indicator.SetActive(false);
        
        if (firstDialogue != null && firstDialogue.givesQuest && firstDialogue.questToGive != null)
            activeQuestForThisNPC = firstDialogue.questToGive;
        else if (questCompleteDialogue != null && questCompleteDialogue.completesQuest && questCompleteDialogue.questToComplete != null)
            activeQuestForThisNPC = questCompleteDialogue.questToComplete;
    }
    
    private void Update()
    {
        if (!playerInRange) return;
        if (SimpleDialogueManager.Instance == null) return;
        if (SimpleDialogueManager.Instance.IsShowing) return;
        if (SimpleDialogueManager.Instance.JustEnded) return;
        
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            StartConversation();
    }
    
    private void StartConversation()
    {
        SimpleDialogue dialogueToShow = GetCurrentDialogue();
        if (dialogueToShow != null)
            SimpleDialogueManager.Instance.ShowDialogue(dialogueToShow, this);
    }
    
    private SimpleDialogue GetCurrentDialogue()
    {
        if (questCompleteDialogue != null && questCompleteDialogue.completesQuest && 
            questCompleteDialogue.questToComplete != null &&
            SimpleQuestManager.Instance != null &&
            SimpleQuestManager.Instance.IsQuestActive(questCompleteDialogue.questToComplete) &&
            SimpleQuestManager.Instance.CanCompleteQuest(questCompleteDialogue.questToComplete))
            return questCompleteDialogue;
        
        if (firstDialogue != null && firstDialogue.completesQuest && 
            firstDialogue.questToComplete != null &&
            SimpleQuestManager.Instance != null &&
            SimpleQuestManager.Instance.IsQuestActive(firstDialogue.questToComplete) &&
            SimpleQuestManager.Instance.CanCompleteQuest(firstDialogue.questToComplete))
            return questCompleteDialogue ?? firstDialogue;
        
        if (questWasGiven && activeQuestForThisNPC != null &&
            SimpleQuestManager.Instance != null &&
            SimpleQuestManager.Instance.IsQuestActive(activeQuestForThisNPC))
            return reminderDialogue ?? firstDialogue;
        
        if (questWasCompleted && postQuestDialogue != null)
            return postQuestDialogue;
        
        if (firstDialogueShown && reminderDialogue != null)
            return reminderDialogue;
        
        return firstDialogue;
    }
    
    public void OnDialogueFinished()
    {
        firstDialogueShown = true;
        
        if (firstDialogue != null && firstDialogue.givesQuest && firstDialogue.questToGive != null)
        {
            if (SimpleQuestManager.Instance != null)
            {
                SimpleQuestManager.Instance.StartQuest(firstDialogue.questToGive);
                questWasGiven = true;
                activeQuestForThisNPC = firstDialogue.questToGive;
                onQuestStarted?.Invoke();
            }
        }
        
        SimpleDialogue completingDialogue = null;
        if (firstDialogue != null && firstDialogue.completesQuest && firstDialogue.questToComplete != null)
            completingDialogue = firstDialogue;
        else if (questCompleteDialogue != null && questCompleteDialogue.completesQuest && questCompleteDialogue.questToComplete != null)
            completingDialogue = questCompleteDialogue;
        
        if (completingDialogue != null && SimpleQuestManager.Instance != null)
        {
            if (SimpleQuestManager.Instance.IsQuestActive(completingDialogue.questToComplete) &&
                SimpleQuestManager.Instance.CanCompleteQuest(completingDialogue.questToComplete))
            {
                DestroyQuestItem(completingDialogue.questToComplete.requiredItemID);
                SimpleQuestManager.Instance.CompleteQuest(completingDialogue.questToComplete);
                questWasCompleted = true;
                onQuestCompleted?.Invoke();
            }
        }
        
        UpdateIndicator();
    }
    
    private void DestroyQuestItem(string itemID)
    {
        PlayerCarry playerCarry = FindObjectOfType<PlayerCarry>();
        if (playerCarry != null && playerCarry.IsCarryingObject)
        {
            PickupableItem carriedItem = playerCarry.CarriedObject?.GetComponent<PickupableItem>();
            if (carriedItem != null && carriedItem.itemID == itemID)
                Destroy(playerCarry.CarriedObject);
        }
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