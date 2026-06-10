using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private string footstepSound = "";
    [SerializeField] private Transform cameraTransform;

    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private bool facingRight = true;
    private bool inputBlocked;
    private Rigidbody2D rb;
    private bool wasMoving;
    private Vector3 originalScale;
    private Vector3 cameraWorldOffset;

    public bool IsMoving => moveInput.magnitude > 0.1f;
    public float MoveSpeed => moveSpeed;

    public bool InputBlocked
    {
        get => inputBlocked;
        set => inputBlocked = value;
    }

    private void Start()
    {
        originalScale = transform.localScale;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.freezeRotation = true;

        if (cameraTransform != null)
        {
            cameraTransform.SetParent(null);
            cameraWorldOffset = cameraTransform.position - transform.position;
        }
    }

    private void Update()
    {
        if (!inputBlocked)
        {
            moveInput = Vector2.zero;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) moveInput.x = -1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveInput.x = 1;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) moveInput.y = 1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) moveInput.y = -1;
            moveInput = moveInput.normalized;
        }
        else
        {
            moveInput = Vector2.zero;
        }

        UpdateFacing();

        bool moving = IsMoving;
        if (moving && !wasMoving)
            PlayFootstep();
        wasMoving = moving;

        if (cameraTransform != null)
            cameraTransform.position = transform.position + cameraWorldOffset;
    }

    private void FixedUpdate()
    {
        Vector2 targetVelocity = moveInput * moveSpeed;
        float accelRate = (targetVelocity.magnitude > 0.01f) ? acceleration : deceleration;
        currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime * accelRate);
        rb.velocity = currentVelocity;
    }

    private void PlayFootstep()
    {
        if (!string.IsNullOrEmpty(footstepSound) && AudioManager.instance != null)
            AudioManager.instance.Play(footstepSound);
    }

    private void UpdateFacing()
    {
        if (moveInput.x > 0 && !facingRight)
        {
            facingRight = true;
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (moveInput.x < 0 && facingRight)
        {
            facingRight = false;
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
    }

    public void SetMoveSpeed(float newSpeed) { moveSpeed = newSpeed; }
}