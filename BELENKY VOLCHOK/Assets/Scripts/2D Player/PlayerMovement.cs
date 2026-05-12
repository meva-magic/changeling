using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 10f;
    
    private float moveInput;
    private float currentVelocity;
    private bool facingRight = true;
    private bool inputBlocked = false;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    
    public bool IsMoving => Mathf.Abs(moveInput) > 0.1f;
    public float MoveSpeed => moveSpeed;
    public bool InputBlocked 
    { 
        get { return inputBlocked; } 
        set { inputBlocked = value; } 
    }
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }
    
    private void Update()
    {
        if (!inputBlocked)
        {
            moveInput = 0;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) moveInput = -1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveInput = 1;
        }
        else
        {
            moveInput = 0;
        }
        
        UpdateFacing();
    }
    
    private void FixedUpdate()
    {
        float targetSpeed = moveInput * moveSpeed;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        currentVelocity = Mathf.Lerp(currentVelocity, targetSpeed, Time.fixedDeltaTime * accelRate);
        
        rb.velocity = new Vector2(currentVelocity, rb.velocity.y);
    }
    
    private void UpdateFacing()
    {
        if (moveInput > 0 && !facingRight) Flip();
        else if (moveInput < 0 && facingRight) Flip();
    }
    
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    public void SetMoveSpeed(float newSpeed) { moveSpeed = newSpeed; }
}