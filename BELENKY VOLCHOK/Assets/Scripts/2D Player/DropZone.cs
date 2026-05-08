using UnityEngine;
using System.Collections;

public class DropZone : MonoBehaviour
{
    [Header("Drop Zone Settings")]
    [SerializeField] private string acceptedItemID = "";          // Leave empty to accept any item
    [SerializeField] private Transform snapPoint;                // Where the item snaps to
    //[SerializeField] private float snapSpeed = 15f;              // How fast item snaps
    [SerializeField] private bool canPickupFromZone = true;      // Can player take item back?
    
    [Header("Events")]
    [SerializeField] private UnityEngine.Events.UnityEvent onItemPlaced;
    [SerializeField] private UnityEngine.Events.UnityEvent onItemRemoved;
    [SerializeField] private UnityEngine.Events.UnityEvent<GameObject> onCorrectItemPlaced;
    [SerializeField] private UnityEngine.Events.UnityEvent<GameObject> onWrongItemPlaced;
    
    private GameObject currentItem;
    private PickupableItem currentPickupable;
    private bool isOccupied;
    
    public bool IsOccupied => isOccupied;
    public GameObject CurrentItem => currentItem;
    public string AcceptedItemID => acceptedItemID;
    
    private void Start()
    {
        if (snapPoint == null)
            snapPoint = transform;
    }
    
    // Called when player drops an item on this zone
    public void PlaceItem(GameObject item)
    {
        PickupableItem pickupable = item.GetComponent<PickupableItem>();
        if (pickupable == null) return;
        
        // Check if this zone accepts this item (or accepts anything)
        bool isCorrectItem = string.IsNullOrEmpty(acceptedItemID) || 
                            pickupable.itemID == acceptedItemID;
        
        if (isOccupied && canPickupFromZone)
        {
            // Zone has an item - SWAP them
            SwapItems(pickupable);
        }
        else if (!isOccupied)
        {
            // Zone is empty - PLACE item
            PlaceNewItem(pickupable, isCorrectItem);
        }
        else
        {
            // Zone occupied and can't pickup - reject
            Debug.Log("Drop zone is occupied and items can't be taken back");
            onWrongItemPlaced?.Invoke(item);
        }
    }
    
    private void PlaceNewItem(PickupableItem pickupable, bool isCorrect)
    {
        currentItem = pickupable.gameObject;
        currentPickupable = pickupable;
        isOccupied = true;
        
        // Snap item to snap point
        StartCoroutine(SnapItemToPoint(pickupable.gameObject, snapPoint.position));
        
        // Disable physics on dropped item
        Rigidbody2D rb = pickupable.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
        
        // Make it non-pickupable while in zone
        pickupable.OnSnapToZone();
        
        // Trigger events
        onItemPlaced?.Invoke();
        
        if (isCorrect)
        {
            onCorrectItemPlaced?.Invoke(currentItem);
            Debug.Log($"Correct item '{pickupable.itemID}' placed in zone '{gameObject.name}'");
        }
        else
        {
            onWrongItemPlaced?.Invoke(currentItem);
            Debug.Log($"Item '{pickupable.itemID}' placed in zone '{gameObject.name}' (accepts: '{acceptedItemID}')");
        }
    }
    
    private void SwapItems(PickupableItem newItem)
    {
        // Take the old item out
        GameObject oldItem = currentItem;
        PickupableItem oldPickupable = currentPickupable;
        
        // Get player reference for giving the old item
        PlayerCarry playerCarry = FindObjectOfType<PlayerCarry>();
        
        if (playerCarry != null && oldPickupable != null)
        {
            // Unsnap old item
            oldPickupable.OnUnsnapFromZone();
            
            // Give old item to player
            oldItem.transform.SetParent(playerCarry.CarryPoint);
            oldItem.transform.localPosition = Vector3.zero;
            oldPickupable.OnPickup(playerCarry.CarryPoint);
            
            Debug.Log($"Swapped: took '{oldPickupable.itemID}', player now carries it");
        }
        else
        {
            // No player to give item to, just drop it
            oldPickupable?.OnUnsnapFromZone();
            oldPickupable?.OnDrop(transform.position + Vector3.up * 1.5f);
        }
        
        // Place the new item
        currentItem = newItem.gameObject;
        currentPickupable = newItem;
        
        StartCoroutine(SnapItemToPoint(newItem.gameObject, snapPoint.position));
        
        Rigidbody2D rb = newItem.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
        
        newItem.OnSnapToZone();
        
        // Check if new item is correct
        bool isCorrect = string.IsNullOrEmpty(acceptedItemID) || 
                        newItem.itemID == acceptedItemID;
        
        onItemPlaced?.Invoke();
        onItemRemoved?.Invoke(); // For the old item being removed
        
        if (isCorrect)
        {
            onCorrectItemPlaced?.Invoke(currentItem);
        }
        else
        {
            onWrongItemPlaced?.Invoke(currentItem);
        }
    }
    
    private IEnumerator SnapItemToPoint(GameObject item, Vector3 targetPosition)
    {
        float elapsed = 0f;
        float duration = 0.3f;
        Vector3 startPos = item.transform.position;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Smooth ease out
            t = 1f - (1f - t) * (1f - t);
            
            item.transform.position = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }
        
        item.transform.position = targetPosition;
    }
    
    // Visual feedback in editor
    private void OnDrawGizmos()
    {
        // Draw snap point
        if (snapPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(snapPoint.position, 0.2f);
            Gizmos.DrawWireCube(snapPoint.position, Vector3.one * 0.3f);
        }
        
        // Draw zone area
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            // Show color based on acceptance
            if (isOccupied)
                Gizmos.color = new Color(0, 1, 0, 0.2f); // Green - occupied
            else if (string.IsNullOrEmpty(acceptedItemID))
                Gizmos.color = new Color(1, 1, 1, 0.2f); // White - accepts anything
            else
                Gizmos.color = new Color(1, 0.5f, 0, 0.2f); // Orange - specific item
                
            Gizmos.DrawCube(transform.position, col.bounds.size);
        }
    }
}