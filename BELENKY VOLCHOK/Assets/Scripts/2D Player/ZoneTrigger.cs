using UnityEngine;
using UnityEngine.Events;

public class ZoneTrigger : MonoBehaviour
{
    [SerializeField] private bool useFadeTransition = true;
    [SerializeField] private UnityEvent onPlayerEnterZone;
    [SerializeField] private UnityEvent onPlayerExitZone;
    [SerializeField] private bool triggerExitOnce;
    
    private bool exitTriggered;
    private ZoneCamera zoneCamera;
    
    private void Start()
    {
        zoneCamera = ZoneCamera.Instance;
        if (zoneCamera == null) zoneCamera = FindObjectOfType<ZoneCamera>();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (zoneCamera != null)
                zoneCamera.MoveToZone(transform.position, useFadeTransition);
            
            onPlayerEnterZone?.Invoke();
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerExitOnce && exitTriggered) return;
        
        exitTriggered = true;
        onPlayerExitZone?.Invoke();
    }
}