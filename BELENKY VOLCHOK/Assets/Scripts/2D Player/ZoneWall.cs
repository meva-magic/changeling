using UnityEngine;
using System.Collections;

public class ZoneWall : MonoBehaviour
{
    [Header("Zones")]
    [SerializeField] private Zone zoneA;
    [SerializeField] private Zone zoneB;
    
    [Header("Spawn Points")]
    [SerializeField] private Transform spawnInZoneA;
    [SerializeField] private Transform spawnInZoneB;
    
    [Header("A to B Conditions")]
    [SerializeField] private string keyForAtoB = "";
    [SerializeField] private SimpleQuest questRequiredForAtoB;
    [SerializeField] private bool blockAfterGettingQuestItemAtoB = true;
    
    [Header("B to A Conditions")]
    [SerializeField] private string keyForBtoA = "";
    [SerializeField] private SimpleQuest questRequiredForBtoA;
    [SerializeField] private bool blockAfterGettingQuestItemBtoA = true;
    
    [Header("Destroy Settings")]
    [SerializeField] private bool destroyAfterQuestComplete = false;
    [SerializeField] private SimpleQuest questToWatch;
    [SerializeField] private GameObject solidBlocker;
    
    [Header("Item Drop Points")]
    [SerializeField] private Transform itemDropPointZoneA;
    [SerializeField] private Transform itemDropPointZoneB;
    
    [Header("Reminders")]
    [SerializeField] [TextArea(1, 3)] private string reminderNoQuestAtoB = "";
    [SerializeField] [TextArea(1, 3)] private string reminderNoKeyAtoB = "";
    [SerializeField] [TextArea(1, 3)] private string reminderQuestItemGotAtoB = "";
    [SerializeField] [TextArea(1, 3)] private string reminderNoQuestBtoA = "";
    [SerializeField] [TextArea(1, 3)] private string reminderNoKeyBtoA = "";
    [SerializeField] [TextArea(1, 3)] private string reminderQuestItemGotBtoA = "";
    [SerializeField] private float reminderDuration = 2f;
    
    private PlayerCarry playerCarry;
    private Coroutine reminderCoroutine;
    private bool isUnlocked;
    private bool questWasStarted;
    private bool questItemWasRetrieved;
    private string questItemID;
    private float lastReminderTime;
    
    private void Start()
    {
        playerCarry = FindObjectOfType<PlayerCarry>();
        
        if (questToWatch != null)
            questItemID = questToWatch.requiredItemID;
    }
    
    private void Update()
    {
        if (destroyAfterQuestComplete && questToWatch != null && !isUnlocked)
        {
            if (SimpleQuestManager.Instance != null)
            {
                bool isActive = SimpleQuestManager.Instance.IsQuestActive(questToWatch);
                
                if (isActive)
                {
                    questWasStarted = true;
                    
                    if (!questItemWasRetrieved && PlayerHasItem(questItemID))
                        questItemWasRetrieved = true;
                }
                
                if (questWasStarted && !isActive)
                    UnlockWall();
            }
        }
    }
    
    private void UnlockWall()
    {
        isUnlocked = true;
        
        if (solidBlocker != null)
            Destroy(solidBlocker);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (isUnlocked)
            {
                Vector3 playerPos = other.transform.position;
                float distToA = Vector3.Distance(playerPos, zoneA.transform.position);
                float distToB = Vector3.Distance(playerPos, zoneB.transform.position);
                
                if (distToA < distToB)
                    zoneA.TransitionToZone(zoneB, spawnInZoneB);
                else
                    zoneB.TransitionToZone(zoneA, spawnInZoneA);
                
                return;
            }
            
            if (!zoneA.isTransitioning && !zoneB.isTransitioning)
            {
                if (Time.time - lastReminderTime >= 1f)
                {
                    Vector3 pos = other.transform.position;
                    float dA = Vector3.Distance(pos, zoneA.transform.position);
                    float dB = Vector3.Distance(pos, zoneB.transform.position);
                    
                    if (dA < dB)
                        TryCrossAtoB();
                    else
                        TryCrossBtoA();
                }
            }
            
            return;
        }
        
        PickupableItem item = other.GetComponent<PickupableItem>();
        if (item == null) item = other.GetComponentInParent<PickupableItem>();
        
        if (item != null && !item.IsBeingCarried)
        {
            MoveItemToCorrectZone(item);
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        PickupableItem item = other.GetComponent<PickupableItem>();
        if (item == null) item = other.GetComponentInParent<PickupableItem>();
        
        if (item != null && !item.IsBeingCarried)
        {
            MoveItemToCorrectZone(item);
        }
    }
    
    private void MoveItemToCorrectZone(PickupableItem item)
    {
        float distToA = Vector3.Distance(item.transform.position, zoneA.transform.position);
        float distToB = Vector3.Distance(item.transform.position, zoneB.transform.position);
        
        if (distToA < distToB)
        {
            if (itemDropPointZoneA != null)
                item.transform.position = itemDropPointZoneA.position;
            else
                item.transform.position = zoneA.transform.position + Vector3.right * 2f;
        }
        else
        {
            if (itemDropPointZoneB != null)
                item.transform.position = itemDropPointZoneB.position;
            else
                item.transform.position = zoneB.transform.position + Vector3.left * 2f;
        }
        
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
    
    private void TryCrossAtoB()
    {
        if (questRequiredForAtoB != null)
        {
            bool questActive = SimpleQuestManager.Instance != null && 
                              SimpleQuestManager.Instance.IsQuestActive(questRequiredForAtoB);
            
            if (!questActive)
            {
                ShowReminder(reminderNoQuestAtoB);
                return;
            }
            
            if (blockAfterGettingQuestItemAtoB && questItemWasRetrieved)
            {
                ShowReminder(reminderQuestItemGotAtoB);
                return;
            }
        }
        
        if (!string.IsNullOrEmpty(keyForAtoB))
        {
            if (!PlayerHasItem(keyForAtoB))
            {
                ShowReminder(reminderNoKeyAtoB);
                return;
            }
        }
        
        zoneA.TransitionToZone(zoneB, spawnInZoneB);
    }
    
    private void TryCrossBtoA()
    {
        if (questRequiredForBtoA != null)
        {
            bool questActive = SimpleQuestManager.Instance != null && 
                              SimpleQuestManager.Instance.IsQuestActive(questRequiredForBtoA);
            
            if (!questActive)
            {
                ShowReminder(reminderNoQuestBtoA);
                return;
            }
            
            if (blockAfterGettingQuestItemBtoA && questItemWasRetrieved)
            {
                ShowReminder(reminderQuestItemGotBtoA);
                return;
            }
        }
        
        if (!string.IsNullOrEmpty(keyForBtoA))
        {
            if (!PlayerHasItem(keyForBtoA))
            {
                ShowReminder(reminderNoKeyBtoA);
                return;
            }
        }
        
        zoneB.TransitionToZone(zoneA, spawnInZoneA);
    }
    
    private bool PlayerHasItem(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return false;
        if (playerCarry == null) return false;
        if (!playerCarry.IsCarryingObject) return false;
        
        GameObject carriedObj = playerCarry.CarriedObject;
        if (carriedObj == null) return false;
        
        PickupableItem item = carriedObj.GetComponent<PickupableItem>();
        if (item == null)
            item = carriedObj.GetComponentInChildren<PickupableItem>();
        
        return item != null && item.itemID == itemID;
    }
    
    private void ShowReminder(string message)
    {
        lastReminderTime = Time.time;
        
        if (reminderCoroutine != null)
            StopCoroutine(reminderCoroutine);
        
        reminderCoroutine = StartCoroutine(ShowReminderRoutine(message));
    }
    
    private IEnumerator ShowReminderRoutine(string message)
    {
        if (SimpleDialogueManager.Instance != null)
            SimpleDialogueManager.Instance.ShowReminder(message);
        
        yield return new WaitForSeconds(reminderDuration);
        
        if (SimpleDialogueManager.Instance != null)
            SimpleDialogueManager.Instance.HideReminder();
        
        reminderCoroutine = null;
    }
}