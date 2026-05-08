using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 10f;
    
    private float moveInput;
    private float currentVelocity;
    private bool facingRight = true;
    
    public bool IsMoving => Mathf.Abs(moveInput) > 0.1f;
    public float MoveSpeed => moveSpeed;
    
    private void Update()
    {
        moveInput = 0;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) moveInput = -1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveInput = 1;
        
        UpdateFacing();
        HandleMovement();
    }
    
    private void HandleMovement()
    {
        float targetSpeed = moveInput * moveSpeed;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        currentVelocity = Mathf.Lerp(currentVelocity, targetSpeed, Time.deltaTime * accelRate);
        transform.position += Vector3.right * currentVelocity * Time.deltaTime;
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
    
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
    
    public float GetOriginalSpeed()
    {
        return moveSpeed;
    }
}