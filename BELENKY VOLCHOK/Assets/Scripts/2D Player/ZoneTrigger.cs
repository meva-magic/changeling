using UnityEngine;
using UnityEngine.Events;

public class ZoneTrigger : MonoBehaviour
{
    [SerializeField] private UnityEvent onPlayerEnterZone;
    [SerializeField] private UnityEvent onPlayerExitZone;
    [SerializeField] private bool triggerExitOnce;
    
    private bool exitTriggered;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check BOTH tag AND layer
        if (!other.CompareTag("Player")) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        
        Debug.Log($"Player entered zone: {gameObject.name}");
        
        ZoneCamera zoneCam = ZoneCamera.Instance;
        if (zoneCam == null) zoneCam = FindObjectOfType<ZoneCamera>();
        
        if (zoneCam != null)
        {
            zoneCam.MoveToZone(transform.position, true);
        }
        
        onPlayerEnterZone?.Invoke();
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (triggerExitOnce && exitTriggered) return;
        
        exitTriggered = true;
        onPlayerExitZone?.Invoke();
    }
}