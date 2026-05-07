using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float radius = 3f;
    [SerializeField] private bool manualMode;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject indicator;
    
    private Quaternion closedRot;
    private Quaternion targetRot;
    private bool isOpen;
    
    void Start()
    {
        closedRot = transform.localRotation;
        targetRot = closedRot;
        if (indicator) indicator.SetActive(false);
    }
    
    void Update()
    {
        if (!player) return;
        
        bool inRange = Vector3.Distance(transform.position, player.position) <= radius;
        
        if (manualMode)
        {
            if (indicator) indicator.SetActive(inRange);
            
            if (inRange && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
            {
                isOpen = !isOpen;
                UpdateTarget();
            }
            
            // Auto-close when player leaves
            if (!inRange && isOpen)
            {
                isOpen = false;
                UpdateTarget();
            }
        }
        else
        {
            if (inRange != isOpen)
            {
                isOpen = inRange;
                UpdateTarget();
            }
        }
        
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * speed);
    }
    
    void UpdateTarget()
    {
        if (isOpen)
        {
            Vector3 doorForward = transform.forward;
            Vector3 toPlayer = (player.position - transform.position).normalized;
            float side = Vector3.Dot(doorForward, toPlayer);
            targetRot = closedRot * Quaternion.Euler(0, side > 0 ? openAngle : -openAngle, 0);
        }
        else
        {
            targetRot = closedRot;
        }
    }
}