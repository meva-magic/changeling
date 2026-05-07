using UnityEngine;

public class MinimalDoomBob : MonoBehaviour
{
    [Header("Walking Bob")]
    [SerializeField] private float walkBobSpeed = 8f;
    [SerializeField] private float walkBobVertical = 0.04f;
    [SerializeField] private float walkBobHorizontal = 0.02f;
    
    [Header("Breathing (Standing Still)")]
    [SerializeField] private float breathSpeed = 1.2f;
    [SerializeField] private float breathAmount = 0.015f;
    [SerializeField] private float breathRandomness = 0.5f;
    
    [Header("Targets")]
    [SerializeField] private Transform cameraTarget;      // Usually the camera
    [SerializeField] private Transform weaponTarget;      // Gun/hands parent
    [SerializeField] private RectTransform[] hudHands;    // UI hands (optional)
    
    private Vector3 camStartPos, weaponStartPos;
    private Vector2[] handsStartPos;
    private float timer;
    private float breathTimer;
    private float randomBreathOffset;
    private float breathSmoothVel;
    private CharacterController controller;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        if(cameraTarget) camStartPos = cameraTarget.localPosition;
        if(weaponTarget) weaponStartPos = weaponTarget.localPosition;
        
        // Store UI hand positions
        if(hudHands != null && hudHands.Length > 0)
        {
            handsStartPos = new Vector2[hudHands.Length];
            for(int i = 0; i < hudHands.Length; i++)
            {
                if(hudHands[i]) handsStartPos[i] = hudHands[i].anchoredPosition;
            }
        }
    }
    
    void Update()
    {
        bool isMoving = IsPlayerMoving();
        
        if(isMoving)
            ApplyWalkBob();
        else
            ApplyBreathingBob();
    }
    
    bool IsPlayerMoving()
    {
        if(controller)
        {
            Vector3 vel = controller.velocity;
            return new Vector3(vel.x, 0, vel.z).magnitude > 0.5f && controller.isGrounded;
        }
        // Fallback if no CharacterController
        return Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
    }
    
    void ApplyWalkBob()
    {
        timer += Time.deltaTime * walkBobSpeed;
        
        float bobY = Mathf.Sin(timer) * walkBobVertical;
        float bobX = Mathf.Cos(timer * 0.5f) * walkBobHorizontal;
        
        // Apply to camera
        if(cameraTarget)
            cameraTarget.localPosition = camStartPos + new Vector3(bobX, bobY, 0);
        
        // Apply to weapon (more pronounced)
        if(weaponTarget)
            weaponTarget.localPosition = weaponStartPos + new Vector3(bobX * 1.5f, bobY * 1.5f, 0);
        
        // Apply to HUD hands
        UpdateHUDHands(bobX * 2f, bobY * 2f);
    }
    
    void ApplyBreathingBob()
    {
        // Smooth random breathing pattern
        breathTimer += Time.deltaTime;
        if(breathTimer > Random.Range(1.5f, 3f))
        {
            breathTimer = 0;
            randomBreathOffset = Random.Range(-breathAmount * breathRandomness, breathAmount * breathRandomness);
        }
        
        // Combine sine wave with randomness
        float breathY = Mathf.Sin(Time.time * breathSpeed) * breathAmount + randomBreathOffset;
        float breathX = Mathf.Cos(Time.time * breathSpeed * 0.7f) * (breathAmount * 0.5f);
        
        // Apply to camera
        if(cameraTarget)
            cameraTarget.localPosition = camStartPos + new Vector3(breathX, breathY, 0);
        
        // Apply to weapon (subtle)
        if(weaponTarget)
            weaponTarget.localPosition = weaponStartPos + new Vector3(breathX, breathY * 1.2f, 0);
        
        // Apply to HUD hands
        UpdateHUDHands(breathX, breathY);
    }
    
    void UpdateHUDHands(float x, float y)
    {
        if(hudHands == null || handsStartPos == null) return;
        
        for(int i = 0; i < hudHands.Length; i++)
        {
            if(hudHands[i])
                hudHands[i].anchoredPosition = handsStartPos[i] + new Vector2(x * 40f, y * 40f);
        }
    }
}