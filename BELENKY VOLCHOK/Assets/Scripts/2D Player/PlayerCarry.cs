using UnityEngine;

public class PlayerCarry : MonoBehaviour
{
    [SerializeField] private Transform carryPoint;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private float slowSpeedMultiplier = 0.6f;
    
    [Header("Indicators")]
    [SerializeField] private GameObject pickupIndicatorPanel;
    [SerializeField] private GameObject dialogueIndicatorPanel;
    
    private GameObject carriedObject;
    private PickupableItem carriedPickupable;
    private PlayerMovement playerMovement;
    private float originalMoveSpeed;
    private PickupableItem nearestItem;
    private Rigidbody2D carriedRb;
    private SimpleDialogueTrigger nearestNPC;
    
    public bool IsCarryingObject => carriedObject != null;
    public GameObject CarriedObject => carriedObject;
    public Transform CarryPoint => carryPoint;
    
    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null) originalMoveSpeed = playerMovement.MoveSpeed;
        HideAllIndicators();
    }
    
    private void Update()
    {
        FindNearestItem();
        FindNearestNPC();
        UpdateIndicators();
        
        if (SimpleDialogueManager.Instance != null && SimpleDialogueManager.Instance.IsShowing)
            return;
        
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (nearestNPC != null)
            {
                nearestNPC.StartConversationPublic();
                return;
            }
            
            if (IsCarryingObject)
            {
                if (nearestItem != null)
                    SwapItems();
                else
                    DropObject();
                return;
            }
            
            if (nearestItem != null)
            {
                TryPickupObject();
                return;
            }
            
            if (TryStartNearbyMinigame()) return;
        }
    }
    
    private void FixedUpdate()
    {
        if (carriedObject != null && carriedRb != null)
            carriedRb.MovePosition(carryPoint.position);
    }
    
    private void HideAllIndicators()
    {
        if (pickupIndicatorPanel != null) pickupIndicatorPanel.SetActive(false);
        if (dialogueIndicatorPanel != null) dialogueIndicatorPanel.SetActive(false);
    }
    
    private void FindNearestNPC()
    {
        nearestNPC = null;
        
        SimpleDialogueTrigger[] npcs = FindObjectsOfType<SimpleDialogueTrigger>();
        foreach (SimpleDialogueTrigger npc in npcs)
        {
            if (!npc.IsPlayerInRange()) continue;
            
            float distance = Vector2.Distance(transform.position, npc.transform.position);
            if (distance <= 3f)
            {
                nearestNPC = npc;
                break;
            }
        }
    }
    
    private void FindNearestItem()
    {
        nearestItem = null;
        float closestDistance = Mathf.Infinity;
        
        GameObject[] itemObjects = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject obj in itemObjects)
        {
            PickupableItem item = obj.GetComponent<PickupableItem>();
            if (item == null) item = obj.GetComponentInChildren<PickupableItem>();
            if (item == null) continue;
            if (item == carriedPickupable || item.IsBeingCarried) continue;
            
            float distance = Vector2.Distance(transform.position, obj.transform.position);
            if (distance <= item.pickupRange && distance < closestDistance)
            {
                closestDistance = distance;
                nearestItem = item;
            }
        }
    }
    
    private void UpdateIndicators()
    {
        bool dialogueActive = SimpleDialogueManager.Instance != null && SimpleDialogueManager.Instance.IsShowing;
        
        if (dialogueActive)
        {
            HideAllIndicators();
            return;
        }
        
        if (nearestNPC != null)
        {
            if (pickupIndicatorPanel != null) pickupIndicatorPanel.SetActive(false);
            if (dialogueIndicatorPanel != null) dialogueIndicatorPanel.SetActive(true);
        }
        else if (nearestItem != null && !IsCarryingObject)
        {
            if (pickupIndicatorPanel != null) pickupIndicatorPanel.SetActive(true);
            if (dialogueIndicatorPanel != null) dialogueIndicatorPanel.SetActive(false);
        }
        else
        {
            HideAllIndicators();
        }
    }
    
    private bool TryStartNearbyMinigame()
    {
        if (IsCarryingObject) return false;
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 3f);
        foreach (Collider2D hit in hits)
        {
            ClickerMinigame minigame = hit.GetComponent<ClickerMinigame>();
            if (minigame != null && minigame.enabled)
            {
                minigame.StartMinigame();
                return true;
            }
        }
        return false;
    }
    
    private void TryPickupObject()
    {
        if (nearestItem == null) return;
        carriedObject = nearestItem.gameObject;
        carriedPickupable = nearestItem;
        carriedRb = carriedObject.GetComponent<Rigidbody2D>();
        nearestItem.OnPickup(carryPoint);
        if (nearestItem.slowsPlayer && playerMovement != null)
            playerMovement.SetMoveSpeed(originalMoveSpeed * slowSpeedMultiplier);
    }
    
    private void SwapItems()
    {
        Vector3 swapPosition = nearestItem.transform.position;
        carriedPickupable.OnDrop(swapPosition);
        
        carriedObject = nearestItem.gameObject;
        carriedPickupable = nearestItem;
        carriedRb = carriedObject.GetComponent<Rigidbody2D>();
        nearestItem.OnPickup(carryPoint);
        
        if (carriedPickupable.slowsPlayer && playerMovement != null)
            playerMovement.SetMoveSpeed(originalMoveSpeed * slowSpeedMultiplier);
        else if (playerMovement != null)
            playerMovement.SetMoveSpeed(originalMoveSpeed);
    }
    
    private void DropObject()
    {
        if (carriedObject == null) return;
        carriedPickupable.OnDrop(dropPoint.position);
        carriedObject = null;
        carriedPickupable = null;
        carriedRb = null;
        if (playerMovement != null) playerMovement.SetMoveSpeed(originalMoveSpeed);
    }
}