using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float gravity = -10f;
    
    [Header("Footsteps")]
    [SerializeField] private string insideStepSound = "footstep_inside";
    [SerializeField] private string outsideStepSound = "footstep_outside";
    
    private CharacterController controller;
    private Vector3 moveDirection;
    private float verticalVelocity;
    private bool isGrounded;
    private bool movementEnabled = true;
    private bool wasMoving = false;
    private bool isInside = false;
    private string currentStepSound = "";
    
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
            Debug.LogError("PlayerMovement: CharacterController не найден!");
    }
    
    private void Update()
    {
        UpdateGroundState();
        HandleInput();
        ApplyGravity();
        ExecuteMovement();
        HandleFootsteps();
    }
    
    private void UpdateGroundState()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;
    }
    
    private void HandleInput()
    {
        if (!movementEnabled) return;
        
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        Vector3 input = new Vector3(horizontal, 0, vertical).normalized;
        moveDirection = transform.TransformDirection(input) * walkSpeed;
    }
    
    private void ApplyGravity()
    {
        verticalVelocity += gravity * Time.deltaTime;
        moveDirection.y = verticalVelocity;
    }
    
    private void ExecuteMovement()
    {
        if (controller != null)
            controller.Move(moveDirection * Time.deltaTime);
    }
    
    private void HandleFootsteps()
    {
        bool isMoving = movementEnabled && isGrounded && moveDirection.magnitude > 0.1f;
        
        string targetSound = isInside ? insideStepSound : outsideStepSound;
        
        if (isMoving && !wasMoving)
        {
            currentStepSound = targetSound;
            AudioManager.instance?.Play(currentStepSound);
        }
        else if (!isMoving && wasMoving)
        {
            AudioManager.instance?.Stop(currentStepSound);
        }
        else if (isMoving && wasMoving && currentStepSound != targetSound)
        {
            AudioManager.instance?.Stop(currentStepSound);
            currentStepSound = targetSound;
            AudioManager.instance?.Play(currentStepSound);
        }
        
        wasMoving = isMoving;
    }
    
    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
        if (!movementEnabled)
        {
            moveDirection = Vector3.zero;
            verticalVelocity = 0;
            if (wasMoving)
            {
                AudioManager.instance?.Stop(currentStepSound);
                wasMoving = false;
            }
        }
    }
    
    public void SetFootstepContext(bool inside)
    {
        isInside = inside;
        if (wasMoving)
        {
            string newSound = isInside ? insideStepSound : outsideStepSound;
            if (currentStepSound != newSound)
            {
                AudioManager.instance?.Stop(currentStepSound);
                currentStepSound = newSound;
                AudioManager.instance?.Play(currentStepSound);
            }
        }
    }
    
    public bool IsMovementEnabled => movementEnabled;
}