using UnityEngine;

public class QuestTriggerZone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string dialogueKey = "quest_start_dialogue";
    [SerializeField] private QuestData questToStart;
    [SerializeField] private float interactionRange = 2f;
    
    [Header("One Time")]
    [SerializeField] private bool destroyAfterTrigger = true;
    
    private bool isActivated;
    private bool isInRange;
    private Transform playerTransform;
    private PlayerMovement playerMovement;
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerMovement = player.GetComponent<PlayerMovement>();
        }
    }
    
    private void Update()
    {
        if (isActivated) return;
        
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance <= interactionRange && !isActivated)
            {
                ActivateQuest();
            }
        }
    }
    
    private void ActivateQuest()
    {
        isActivated = true;
        
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false);
        
        DialogueSystem.Instance.ShowDialogue(dialogueKey, OnDialogueComplete);
    }
    
    private void OnDialogueComplete()
    {
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);
        
        QuestTracker questTracker = ServiceLocator.Get<QuestTracker>();
        if (questTracker != null && questToStart != null)
        {
            questTracker.BeginQuest(questToStart);
        }
        
        if (destroyAfterTrigger)
            Destroy(gameObject);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
