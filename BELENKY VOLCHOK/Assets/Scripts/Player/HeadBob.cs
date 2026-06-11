using UnityEngine;

public class DoomBob : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private RectTransform[] hands;
    [SerializeField] private float walkBobAmount = 0.02f;
    [SerializeField] private float idleBobAmount = 0.005f;
    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] private float idleSpeed = 0.5f;
    [SerializeField] private float handSwayAmount = 8f;
    [SerializeField] private float idleHandSwayAmount = 3f;
    
    private Vector3 camStart;
    private Vector2[] handsStart;
    private float walkTimer;
    private float idleTimer;
    private CharacterController controller;
    private bool wasMoving = false;
    private bool isMovementEnabled = true;
    
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            controller = playerObj.GetComponent<CharacterController>();
        
        if (cameraTarget == null)
            cameraTarget = transform;
        
        camStart = cameraTarget.localPosition;
        
        if (hands != null && hands.Length > 0)
        {
            handsStart = new Vector2[hands.Length];
            for (int i = 0; i < hands.Length; i++)
            {
                if (hands[i] != null)
                    handsStart[i] = hands[i].anchoredPosition;
            }
        }
    }
    
    void Update()
    {
        // Проверяем, активна ли мини-игра
        MinigameStarter minigame = ServiceLocator.Get<MinigameStarter>();
        if (minigame != null && minigame.IsMinigameActive)
        {
            if (cameraTarget != null)
                cameraTarget.localPosition = camStart;
            return;
        }
        
        // Проверяем, может ли игрок двигаться
        PlayerMovement movement = GetComponentInParent<PlayerMovement>();
        if (movement != null)
            isMovementEnabled = movement.IsMovementEnabled;
        
        bool moving = !isMovementEnabled ? false :
                      controller != null && controller.isGrounded && 
                      new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude > 0.5f;
        
        if (moving)
        {
            walkTimer += Time.deltaTime * walkSpeed;
            float y = Mathf.Sin(walkTimer) * walkBobAmount;
            float x = Mathf.Cos(walkTimer * 0.5f) * walkBobAmount;
            
            if (cameraTarget != null)
                cameraTarget.localPosition = camStart + new Vector3(x, y, 0);
            
            if (handsStart != null)
            {
                for (int i = 0; i < hands.Length; i++)
                {
                    if (hands[i] == null) continue;
                    float swayX = Mathf.Sin(walkTimer * 0.7f + i) * handSwayAmount;
                    float swayY = Mathf.Cos(walkTimer * 0.5f + i) * handSwayAmount * 0.3f;
                    hands[i].anchoredPosition = handsStart[i] + new Vector2(x * 40f + swayX, y * 40f + swayY);
                }
            }
            
            wasMoving = true;
        }
        else
        {
            idleTimer += Time.deltaTime * idleSpeed;
            float y = Mathf.Sin(idleTimer) * idleBobAmount;
            float x = Mathf.Cos(idleTimer * 0.5f) * idleBobAmount;
            
            if (cameraTarget != null)
                cameraTarget.localPosition = camStart + new Vector3(x, y, 0);
            
            if (handsStart != null)
            {
                for (int i = 0; i < hands.Length; i++)
                {
                    if (hands[i] == null) continue;
                    
                    if (wasMoving)
                    {
                        wasMoving = false;
                        idleTimer = 0f;
                    }
                    
                    float swayX = Mathf.Sin(idleTimer * 0.7f + i) * idleHandSwayAmount;
                    float swayY = Mathf.Cos(idleTimer * 0.5f + i) * idleHandSwayAmount * 0.3f;
                    hands[i].anchoredPosition = handsStart[i] + new Vector2(x * 40f + swayX, y * 40f + swayY);
                }
            }
        }
    }
}