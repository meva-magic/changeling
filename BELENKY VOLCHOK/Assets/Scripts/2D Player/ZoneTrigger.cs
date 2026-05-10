using UnityEngine;
using UnityEngine.Events;

public class ZoneTrigger : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private bool useFadeTransition = true;
    
    [Header("Events")]
    [SerializeField] private UnityEvent onPlayerEnterZone;
    [SerializeField] private UnityEvent onPlayerExitZone;
    [SerializeField] private bool triggerExitOnce;
    
    private bool exitTriggered;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        ZoneCamera zoneCam = ZoneCamera.Instance;
        if (zoneCam == null) zoneCam = FindObjectOfType<ZoneCamera>();
        
        if (zoneCam != null)
            zoneCam.MoveToZone(transform.position, useFadeTransition);
        
        onPlayerEnterZone?.Invoke();
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        if (triggerExitOnce && exitTriggered) return;
        
        exitTriggered = true;
        onPlayerExitZone?.Invoke();
    }
}