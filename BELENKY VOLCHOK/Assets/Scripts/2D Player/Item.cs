using UnityEngine;
using UnityEngine.Events;

public class PickupableItem : MonoBehaviour
{
    public string itemID;
    public bool slowsPlayer;
    public float pickupRange = 2f;
    
    [Header("Events")]
    public UnityEvent onPickupEvent;
    
    private bool isBeingCarried;
    private Collider2D itemCollider;
    
    public bool IsBeingCarried => isBeingCarried;
    
    private void Awake()
    {
        itemCollider = GetComponent<Collider2D>();
    }
    
    public void OnPickup(Transform carryPoint)
    {
        isBeingCarried = true;
        transform.SetParent(carryPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        
        if (itemCollider != null) itemCollider.enabled = false;
        
        // Fire event
        onPickupEvent?.Invoke();
        Debug.Log($"[PickupableItem] Picked up: {itemID}, event fired: {onPickupEvent.GetPersistentEventCount()} listeners");
    }
    
    public void OnDrop(Vector3 position)
    {
        isBeingCarried = false;
        transform.SetParent(null);
        transform.position = position;
        transform.rotation = Quaternion.identity;
        
        if (itemCollider != null) itemCollider.enabled = true;
    }
}