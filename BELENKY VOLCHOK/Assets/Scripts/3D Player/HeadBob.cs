using UnityEngine;

public class DoomBob : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private RectTransform[] hands;
    [SerializeField] private float walkBobAmount = 0.1f;
    [SerializeField] private float idleBobAmount = 0.02f;
    [SerializeField] private float idleSpeed = 0.5f;
    [SerializeField] private float handSwayAmount = 8f;
    
    private Vector3 camStart;
    private Vector2[] handsStart;
    private float timer;
    private float idleTimer;
    private CharacterController controller;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        if(cameraTarget == null)
        {
            enabled = false;
            return;
        }
        
        camStart = cameraTarget.localPosition;
        
        if(hands != null && hands.Length > 0)
        {
            handsStart = new Vector2[hands.Length];
            for(int i = 0; i < hands.Length; i++)
            {
                if(hands[i] != null)
                    handsStart[i] = hands[i].anchoredPosition;
            }
        }
    }
    
    void Update()
    {
        if(cameraTarget == null) return;
        
        bool moving = controller && controller.isGrounded && 
                      new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude > 0.5f;
        
        if(moving)
        {
            timer += Time.deltaTime * 8f;
            float y = Mathf.Sin(timer) * walkBobAmount;
            float x = Mathf.Cos(timer * 0.5f) * walkBobAmount;
            
            cameraTarget.localPosition = camStart + new Vector3(x, y, 0);
            
            if(handsStart != null)
            {
                for(int i = 0; i < hands.Length; i++)
                {
                    if(hands[i] == null) continue;
                    float swayX = Mathf.Sin(timer * 0.7f + i) * handSwayAmount;
                    float swayY = Mathf.Cos(timer * 0.5f + i) * handSwayAmount * 0.3f;
                    hands[i].anchoredPosition = handsStart[i] + new Vector2(x * 40f + swayX, y * 40f + swayY);
                }
            }
        }
        else
        {
            idleTimer += Time.deltaTime * idleSpeed;
            float y = Mathf.Sin(idleTimer) * idleBobAmount;
            float x = Mathf.Cos(idleTimer * 0.5f) * idleBobAmount;
            
            cameraTarget.localPosition = camStart + new Vector3(x, y, 0);
            
            if(handsStart != null)
            {
                for(int i = 0; i < hands.Length; i++)
                {
                    if(hands[i] == null) continue;
                    float swayX = Mathf.Sin(idleTimer * 0.7f + i) * handSwayAmount * 0.6f;
                    float swayY = Mathf.Cos(idleTimer * 0.5f + i) * handSwayAmount * 0.2f;
                    hands[i].anchoredPosition = handsStart[i] + new Vector2(x * 40f + swayX, y * 40f + swayY);
                }
            }
        }
    }
}