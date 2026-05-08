using UnityEngine;

public class PlayerCarry : MonoBehaviour
{
    [SerializeField] private Transform carryPoint;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private float slowSpeedMultiplier = 0.6f;
    [SerializeField] private GameObject pickupIndicatorUI;
    
    private GameObject carriedObject;
    private PickupableItem carriedPickupable;
    private PlayerMovement playerMovement;
    private float originalMoveSpeed;
    private PickupableItem nearestItem;
    
    public bool IsCarryingObject => carriedObject != null;
    public GameObject CarriedObject => carriedObject;
    public Transform CarryPoint => carryPoint;
    
    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null) originalMoveSpeed = playerMovement.MoveSpeed;
        if (pickupIndicatorUI != null) pickupIndicatorUI.SetActive(false);
    }
    
    private void Update()
    {
        FindNearestItem();
        UpdatePickupIndicator();
        
        if (SimpleDialogueManager.Instance != null && SimpleDialogueManager.Instance.IsShowing)
            return;
        
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (IsNearNPC()) return;
            if (TryStartNearbyMinigame()) return;
            
            if (IsCarryingObject)
            {
                if (nearestItem != null) SwapItems();
                else DropObject();
            }
            else
            {
                TryPickupObject();
            }
        }
    }
    
    private bool IsNearNPC()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 3f);
        foreach (Collider2D hit in hits)
        {
            if (hit.GetComponent<SimpleDialogueTrigger>() != null)
                return true;
        }
        return false;
    }
    
    private void FindNearestItem()
    {
        nearestItem = null;
        float closestDistance = Mathf.Infinity;
        PickupableItem[] allItems = FindObjectsOfType<PickupableItem>();
        
        foreach (PickupableItem item in allItems)
        {
            if (item == carriedPickupable || item.IsBeingCarried) continue;
            float distance = Vector2.Distance(transform.position, item.transform.position);
            if (distance <= item.pickupRange && distance < closestDistance)
            {
                closestDistance = distance;
                nearestItem = item;
            }
        }
    }
    
    private void UpdatePickupIndicator()
    {
        if (pickupIndicatorUI != null)
        {
            bool show = nearestItem != null && !IsCarryingObject &&
                       (SimpleDialogueManager.Instance == null || !SimpleDialogueManager.Instance.IsShowing);
            pickupIndicatorUI.SetActive(show);
        }
    }
    
    private bool TryStartNearbyMinigame()
    {
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
        nearestItem.OnPickup(carryPoint);
        if (nearestItem.slowsPlayer && playerMovement != null)
            playerMovement.SetMoveSpeed(originalMoveSpeed * slowSpeedMultiplier);
        if (pickupIndicatorUI != null) pickupIndicatorUI.SetActive(false);
    }
    
    private void SwapItems()
    {
        Vector3 swapPosition = nearestItem.transform.position;
        carriedPickupable.OnDrop(swapPosition);
        carriedObject = nearestItem.gameObject;
        carriedPickupable = nearestItem;
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
        if (playerMovement != null) playerMovement.SetMoveSpeed(originalMoveSpeed);
    }
}