using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb2D;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float acceleration = 20f;
    public float deceleration = 30f;

    [Header("Jump Settings")]
    public float jumpForce = 10f;

    [Header("Ground Detection")]
    public float groundRayLength = 0.2f;
    public LayerMask groundLayer;
    public Vector2 groundRayOffset = new Vector2(0f, -0.5f);

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip skidClip;

    [Header("Visual Effects")]
    public ParticleSystem skidParticles;

    private float velocityX = 0f;
    private float previousVelocityX = 0f;

    // Flip delay logic
    private float flipDelayTimer = 0f;
    private float flipDelayDuration = 0.1f;
    private int lastMoveDirection = 1;
    private Vector3 originalScale;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
    }

    private void HandleMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        float targetSpeed = moveInput * moveSpeed;

        // Detect direction change at full speed
        bool inputChangedDirection = Mathf.Sign(moveInput) != Mathf.Sign(previousVelocityX) && moveInput != 0;
        bool wasAtFullSpeed = Mathf.Abs(previousVelocityX) >= moveSpeed * 0.9f;

        if (inputChangedDirection && wasAtFullSpeed && IsGrounded())
        {
            if (!audioSource.isPlaying && skidClip != null)
            {
                audioSource.PlayOneShot(skidClip);
            }

            if (skidParticles != null)
            {
                skidParticles.Play();
            }

            flipDelayTimer = flipDelayDuration;
            lastMoveDirection = (int)Mathf.Sign(moveInput);
        }

        // Apply acceleration/deceleration
        if (moveInput != 0)
        {
            velocityX = Mathf.MoveTowards(velocityX, targetSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            velocityX = Mathf.MoveTowards(velocityX, 0f, deceleration * Time.deltaTime);
        }

        // Handle delayed sprite flip
        if (flipDelayTimer > 0f)
        {
            flipDelayTimer -= Time.deltaTime;
            if (flipDelayTimer <= 0f)
            {
                transform.localScale = new Vector3(originalScale.x * lastMoveDirection, originalScale.y, originalScale.z);
            }
        }
        else if (moveInput != 0)
        {
            transform.localScale = new Vector3(originalScale.x * Mathf.Sign(moveInput), originalScale.y, originalScale.z);
        }

        previousVelocityX = velocityX;
        rb2D.linearVelocity = new Vector2(velocityX, rb2D.linearVelocity.y);
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Spacebar pressed");

            if (IsGrounded())
            {
                rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jumpForce);
            }
        }
    }

    private bool IsGrounded()
    {
        Vector2 origin = (Vector2)transform.position + groundRayOffset;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundRayLength, groundLayer);
        Debug.DrawRay(origin, Vector2.down * groundRayLength, Color.green);

        return hit.collider != null;
    }

    void OnDrawGizmosSelected()
    {
        Vector2 origin = (Vector2)transform.position + groundRayOffset;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + Vector2.down * groundRayLength);
    }
}