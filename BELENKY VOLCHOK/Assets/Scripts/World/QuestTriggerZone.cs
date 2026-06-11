using UnityEngine;

public class QuestTriggerZone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string dialogueKey = "quest.startFirewood";
    [SerializeField] private QuestData questToStart;
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private bool destroyAfterTrigger = true;
    
    private bool isActivated;
    private Transform playerTransform;
    private PlayerMovement playerMovement;
    private float checkTimer = 0f;
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerMovement = player.GetComponent<PlayerMovement>();
        }
        
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = interactionRange;
        }
        else
        {
            col.isTrigger = true;
        }
    }
    
    private void Update()
    {
        if (isActivated) return;
        if (playerTransform == null) return;
        
        checkTimer += Time.deltaTime;
        if (checkTimer >= 0.2f)
        {
            checkTimer = 0f;
            
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance <= interactionRange)
            {
                ActivateQuest();
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (isActivated) return;
        
        if (other.CompareTag("Player"))
        {
            ActivateQuest();
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (isActivated) return;
        
        if (other.CompareTag("Player"))
        {
            ActivateQuest();
        }
    }
    
    private void ActivateQuest()
    {
        if (isActivated) return;
        isActivated = true;
        
        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(false);
        }
        
        if (DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.ShowDialogue(dialogueKey, OnDialogueComplete);
        }
        else
        {
            OnDialogueComplete();
        }
    }
    
    private void OnDialogueComplete()
    {
        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(true);
        }
        
        if (questToStart != null)
        {
            QuestTracker questTracker = ServiceLocator.Get<QuestTracker>();
            if (questTracker != null)
            {
                questTracker.BeginQuest(questToStart);
            }
        }
        
        if (destroyAfterTrigger)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}