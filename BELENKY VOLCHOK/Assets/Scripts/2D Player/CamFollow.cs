using UnityEngine;

public class FollowPlayerCamera : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -10);
    [SerializeField] private bool lockY = true;
    [SerializeField] private float yPosition = 0f;
    
    private Transform player;
    private Vector3 velocity = Vector3.zero;
    
    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }
    
    private void LateUpdate()
    {
        if (player == null) return;
        
        Vector3 targetPos = player.position + offset;
        
        if (lockY)
            targetPos.y = yPosition;
        
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            1f / followSpeed
        );
    }
}