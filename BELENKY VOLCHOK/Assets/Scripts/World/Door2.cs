using UnityEngine;

public class Door2 : MonoBehaviour
{
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float radius = 3f;
    [SerializeField] private bool manualMode;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject indicator;
    
    [Header("Close Delay")]
    [SerializeField] private float closeDelay = 1f;
    
    private Quaternion closedRot;
    private Quaternion targetRot;
    private bool isOpen;
    private bool hasEntered = false;
    private float exitTimer = 0f;
    
    void Start()
    {
        closedRot = transform.localRotation;
        targetRot = closedRot;
        if (indicator) indicator.SetActive(false);
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }
    
    void Update()
    {
        if (!player) return;
        
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0;
        float distance = toPlayer.magnitude;
        bool inRange = distance <= radius;
        
        if (manualMode)
        {
            if (indicator) indicator.SetActive(inRange);
            
            if (inRange && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
            {
                isOpen = !isOpen;
                UpdateTarget();
                exitTimer = 0f;
            }
            
            if (!inRange && isOpen)
            {
                if (exitTimer <= 0f)
                    exitTimer = closeDelay;
                
                exitTimer -= Time.deltaTime;
                if (exitTimer <= 0f)
                {
                    isOpen = false;
                    UpdateTarget();
                    hasEntered = false;
                }
            }
        }
        else
        {
            if (inRange && !hasEntered)
            {
                hasEntered = true;
                isOpen = true;
                UpdateTargetBySide();
                exitTimer = 0f;
            }
            else if (!inRange && hasEntered)
            {
                if (exitTimer <= 0f)
                    exitTimer = closeDelay;
                
                exitTimer -= Time.deltaTime;
                if (exitTimer <= 0f)
                {
                    hasEntered = false;
                    isOpen = false;
                    UpdateTarget();
                }
            }
        }
        
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * speed);
    }
    
    void UpdateTargetBySide()
    {
        Vector3 doorPos = transform.position;
        Vector3 playerPos = player.position;
        playerPos.y = doorPos.y;
        
        Vector3 toPlayer = (playerPos - doorPos).normalized;
        Vector3 doorRight = transform.right;
        
        float dot = Vector3.Dot(doorRight, toPlayer);
        
        float angle = dot > 0 ? -openAngle : openAngle;
        
        targetRot = closedRot * Quaternion.Euler(0, angle, 0);
    }
    
    void UpdateTarget()
    {
        targetRot = isOpen ? closedRot * Quaternion.Euler(0, openAngle, 0) : closedRot;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
        
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.right * 1f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -transform.right * 1f);
    }
}