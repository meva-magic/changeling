using UnityEngine;

public class DoomBob : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private RectTransform[] hands;
    [SerializeField] private float walkBobAmount = 20f;
    [SerializeField] private float idleBobAmount = 4f;
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float idleSpeed = 0.6f;
    [SerializeField] private float handSwayAmount = 20f;
    [SerializeField] private float handSwaySpeed = 0.7f;
    
    private Vector3 camStart;
    private Vector2[] handsStart;
    private float walkTimer;
    private float idleTimer;
    private float[] noiseOffsets;
    private Transform playerTransform;
    private bool isMoving;
    
    void Start()
    {
        if(cameraTarget == null)
        {
            enabled = false;
            return;
        }
        
        playerTransform = transform.parent;
        if(playerTransform == null)
        {
            enabled = false;
            return;
        }
        
        camStart = cameraTarget.localPosition;
        
        if(hands != null && hands.Length > 0)
        {
            handsStart = new Vector2[hands.Length];
            noiseOffsets = new float[hands.Length];
            for(int i = 0; i < hands.Length; i++)
            {
                if(hands[i] != null)
                {
                    handsStart[i] = hands[i].anchoredPosition;
                    noiseOffsets[i] = Random.Range(0f, 100f);
                }
            }
        }
    }
    
    void Update()
    {
        if(cameraTarget == null || playerTransform == null) return;
        
        // Simple input-based movement detection - most reliable
        isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
        
        if(isMoving)
        {
            idleTimer = 0f;
            walkTimer += Time.deltaTime * walkSpeed;
            
            float y = Mathf.Sin(walkTimer) * walkBobAmount;
            float x = Mathf.Cos(walkTimer * 0.5f) * walkBobAmount * 0.5f;
            
            cameraTarget.localPosition = camStart + new Vector3(x * 0.01f, y * 0.01f, 0);
            
            if(handsStart != null)
            {
                for(int i = 0; i < hands.Length; i++)
                {
                    if(hands[i] == null) continue;
                    
                    float noiseX = Mathf.PerlinNoise(Time.time * handSwaySpeed + noiseOffsets[i], 0f) * 2f - 1f;
                    float noiseY = Mathf.PerlinNoise(0f, Time.time * handSwaySpeed + noiseOffsets[i]) * 2f - 1f;
                    
                    float swayX = Mathf.Sin(walkTimer * handSwaySpeed + i * 2.5f) * handSwayAmount + noiseX * handSwayAmount * 0.5f;
                    float swayY = Mathf.Cos(walkTimer * handSwaySpeed * 0.6f + i * 2.5f) * handSwayAmount * 0.3f + noiseY * handSwayAmount * 0.3f;
                    
                    hands[i].anchoredPosition = handsStart[i] + new Vector2(x * 0.6f + swayX, y * 0.6f + swayY);
                }
            }
        }
        else
        {
            walkTimer = 0f;
            idleTimer += Time.deltaTime * idleSpeed;
            
            float y = Mathf.Sin(idleTimer) * idleBobAmount;
            float x = Mathf.Cos(idleTimer * 0.6f) * idleBobAmount * 0.4f;
            
            cameraTarget.localPosition = camStart + new Vector3(x * 0.005f, y * 0.005f, 0);
            
            if(handsStart != null)
            {
                for(int i = 0; i < hands.Length; i++)
                {
                    if(hands[i] == null) continue;
                    
                    float noiseX = Mathf.PerlinNoise(Time.time * handSwaySpeed * 0.5f + noiseOffsets[i], 0f) * 2f - 1f;
                    float noiseY = Mathf.PerlinNoise(0f, Time.time * handSwaySpeed * 0.5f + noiseOffsets[i]) * 2f - 1f;
                    
                    float swayX = Mathf.Sin(idleTimer * handSwaySpeed * 0.5f + i * 2.5f) * handSwayAmount * 0.8f + noiseX * handSwayAmount * 0.7f;
                    float swayY = Mathf.Cos(idleTimer * handSwaySpeed * 0.4f + i * 2.5f) * handSwayAmount * 0.4f + noiseY * handSwayAmount * 0.5f;
                    
                    hands[i].anchoredPosition = handsStart[i] + new Vector2(x + swayX, y + swayY);
                }
            }
        }
    }
}