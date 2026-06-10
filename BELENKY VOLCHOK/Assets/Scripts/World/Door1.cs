using UnityEngine;

public class Door1 : MonoBehaviour
{
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private Transform player;
    
    [Header("Close Delay")]
    [SerializeField] private float closeDelay = 1f;
    
    private Quaternion closedRot;
    private Quaternion targetRot;
    private Camera playerCamera;
    private bool isOpen = false;
    private float closeTimer = 0f;
    private bool shouldClose = false;
    
    void Start()
    {
        closedRot = transform.localRotation;
        targetRot = closedRot;
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
        
        if (player != null)
        {
            playerCamera = player.GetComponentInChildren<Camera>();
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        bool shouldOpen = distance <= radius;
        
        if (shouldOpen)
        {
            shouldClose = false;
            closeTimer = 0f;
            
            if (!isOpen)
            {
                isOpen = true;
                float angle = CalculateOpenAngle();
                targetRot = closedRot * Quaternion.Euler(0, angle, 0);
            }
        }
        else if (isOpen && !shouldClose)
        {
            shouldClose = true;
            closeTimer = closeDelay;
        }
        
        if (shouldClose)
        {
            closeTimer -= Time.deltaTime;
            if (closeTimer <= 0f)
            {
                isOpen = false;
                targetRot = closedRot;
                shouldClose = false;
            }
        }
        
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * speed);
    }
    
    float CalculateOpenAngle()
    {
        Vector3 playerForward = playerCamera != null ? playerCamera.transform.forward : player.forward;
        playerForward.y = 0;
        playerForward.Normalize();
        
        Vector3 doorForward = transform.forward;
        doorForward.y = 0;
        doorForward.Normalize();
        
        float dot = Vector3.Dot(doorForward, playerForward);
        
        if (dot > 0)
            return -openAngle;
        else
            return openAngle;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
    }
}