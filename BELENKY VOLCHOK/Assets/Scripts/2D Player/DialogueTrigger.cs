using UnityEngine;

public class SimpleDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Assets")]
    [SerializeField] private SimpleDialogue firstDialogue;
    [SerializeField] private SimpleDialogue reminderDialogue;
    [SerializeField] private SimpleDialogue questCompleteDialogue;
    [SerializeField] private SimpleDialogue postQuestDialogue;
    
    [Header("Item Drop Point")]
    [SerializeField] private Transform npcItemDropPoint;
    
    private bool playerInRange;
    private bool firstDialogueShown;
    private bool questWasGiven;
    private bool questWasCompleted;
    private SimpleQuest activeQuestForThisNPC;
    
    private void Start()
    {
        if (firstDialogue != null && firstDialogue.givesQuest && firstDialogue.questToGive != null)
            activeQuestForThisNPC = firstDialogue.questToGive;
        else if (questCompleteDialogue != null && questCompleteDialogue.completesQuest && questCompleteDialogue.questToComplete != null)
            activeQuestForThisNPC = questCompleteDialogue.questToComplete;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
        
        PickupableItem item = other.GetComponent<PickupableItem>();
        if (item == null) item = other.GetComponentInParent<PickupableItem>();
        
        if (item != null && !item.IsBeingCarried)
        {
            MoveItemOutside(item);
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        PickupableItem item = other.GetComponent<PickupableItem>();
        if (item == null) item = other.GetComponentInParent<PickupableItem>();
        
        if (item != null && !item.IsBeingCarried)
        {
            MoveItemOutside(item);
        }
    }
    
    private void MoveItemOutside(PickupableItem item)
    {
        if (npcItemDropPoint != null)
        {
            item.transform.position = npcItemDropPoint.position;
        }
        else
        {
            Vector3 direction = (item.transform.position - transform.position).normalized;
            if (direction.magnitude < 0.1f) direction = Vector3.right;
            item.transform.position = transform.position + direction * 2f;
        }
        
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
    
    public void StartConversationPublic()
    {
        if (SimpleDialogueManager.Instance == null) return;
        if (SimpleDialogueManager.Instance.IsShowing) return;
        if (SimpleDialogueManager.Instance.JustEnded) return;
        if (!playerInRange) return;
        
        SimpleDialogue dialogueToShow = GetCurrentDialogue();
        if (dialogueToShow != null)
        {
            SimpleDialogueManager.Instance.ShowDialogue(dialogueToShow, this);
        }
    }
    
    private SimpleDialogue GetCurrentDialogue()
    {
        if (questCompleteDialogue != null && questCompleteDialogue.completesQuest && 
            questCompleteDialogue.questToComplete != null &&
            SimpleQuestManager.Instance != null &&
            SimpleQuestManager.Instance.IsQuestActive(questCompleteDialogue.questToComplete) &&
            SimpleQuestManager.Instance.CanCompleteQuest(questCompleteDialogue.questToComplete))
        {
            return questCompleteDialogue;
        }
        
        if (firstDialogue != null && firstDialogue.completesQuest && 
            firstDialogue.questToComplete != null &&
            SimpleQuestManager.Instance != null &&
            SimpleQuestManager.Instance.IsQuestActive(firstDialogue.questToComplete) &&
            SimpleQuestManager.Instance.CanCompleteQuest(firstDialogue.questToComplete))
        {
            return questCompleteDialogue ?? firstDialogue;
        }
        
        if (questWasGiven && activeQuestForThisNPC != null &&
            SimpleQuestManager.Instance != null &&
            SimpleQuestManager.Instance.IsQuestActive(activeQuestForThisNPC))
        {
            return reminderDialogue ?? firstDialogue;
        }
        
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
                
                SwapBabyForChangeling();
            }
        }
    }
    
    private void SwapBabyForChangeling()
    {
        FairyJumpscare jumpscare = FindObjectOfType<FairyJumpscare>();
        if (jumpscare != null)
        {
            jumpscare.SwapBabyOnly();
        }
    }
    
    private void DestroyQuestItem(string itemID)
    {
        PlayerCarry playerCarry = FindObjectOfType<PlayerCarry>();
        if (playerCarry != null && playerCarry.IsCarryingObject)
        {
            PickupableItem carriedItem = playerCarry.CarriedObject?.GetComponent<PickupableItem>();
            if (carriedItem != null && carriedItem.itemID == itemID)
            {
                Destroy(playerCarry.CarriedObject);
            }
        }
    }
    
    public bool IsPlayerInRange() { return playerInRange; }
}