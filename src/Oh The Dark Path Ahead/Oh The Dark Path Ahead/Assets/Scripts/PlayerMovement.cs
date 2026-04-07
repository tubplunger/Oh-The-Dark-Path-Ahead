using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 50f;
    public float deceleration = 60f;
    public float airControl = 0.5f;

    [Header("Jump")]
    public float jumpForce = 12f;
    public float jumpCutMultiplier = 0.5f;
    public float fallMultiplier = 2f;

    [Header("Coyote Time")]
    public float coyoteTime = 0.1f;
    private float coyoteTimeCounter;

    [Header("Jump Buffer")]
    public float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Wall Climbing")]
    public Transform frontCheck;
    public Transform backCheck;
    public float wallCheckRadius = 0.2f;
    public LayerMask climbableLayer;

    public float climbSpeed = 5f;
    public float wallJumpForce = 10f;

    [Header("Wall Jump Control")]
    public float wallJumpLockTime = 0.2f;
    private float wallJumpLockCounter;

    private Rigidbody2D rb;

    private float moveInput;
    private bool isGrounded;

    private bool isTouchingWallFront;
    private bool isTouchingWallBack;
    private bool isTouchingWall;

    private bool isClimbing;

    private float currentClimbSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // INPUT
        moveInput = Input.GetAxisRaw("Horizontal");

        // GROUND CHECK
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // WALL CHECK
        isTouchingWallFront = Physics2D.OverlapCircle(frontCheck.position, wallCheckRadius, climbableLayer);
        isTouchingWallBack = Physics2D.OverlapCircle(backCheck.position, wallCheckRadius, climbableLayer);
        isTouchingWall = isTouchingWallFront || isTouchingWallBack;

        // COYOTE TIME
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        // JUMP BUFFER
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // START / STOP CLIMBING
        if (Input.GetKeyDown(KeyCode.E) && isTouchingWall)
        {
            isClimbing = !isClimbing;
        }

        // AUTO EXIT CLIMB
        if (!isTouchingWall)
        {
            isClimbing = false;
        }

        // JUMP (NORMAL)
        if (!isClimbing && jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            Jump();
            jumpBufferCounter = 0f;
        }

        // WALL JUMP
        if (isClimbing && Input.GetButtonDown("Jump"))
        {
            isClimbing = false;

            float direction = isTouchingWallFront ? -1 : 1;
            rb.velocity = new Vector2(direction * wallJumpForce, jumpForce);

            wallJumpLockCounter = wallJumpLockTime;
        }

        // VARIABLE JUMP HEIGHT
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
        }

        // CLIMBING MOVEMENT
        if (isClimbing)
        {
            rb.gravityScale = 0f;

            float vertical = Input.GetAxisRaw("Vertical");
            rb.velocity = new Vector2(0f, vertical * currentClimbSpeed);
        }
        else
        {
            rb.gravityScale = 4f;
        }

        // WALL JUMP LOCK TIMER 
        if (wallJumpLockCounter > 0)
        {
            wallJumpLockCounter -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (!isClimbing)
        {
            if (wallJumpLockCounter <= 0)
            {
                Move();
            }

            ApplyBetterGravity();
        }
    }

    void Move()
    {
        float targetSpeed = moveInput * moveSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f)
            ? acceleration
            : deceleration;

        if (!isGrounded)
            accelRate *= airControl;

        float movement = speedDiff * accelRate;

        rb.AddForce(Vector2.right * movement);
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    void ApplyBetterGravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    void LateUpdate()
    {
        HandleClimbSurface();
    }

    void HandleClimbSurface()
    {
        currentClimbSpeed = climbSpeed;

        Collider2D wallCollider = Physics2D.OverlapCircle(frontCheck.position, wallCheckRadius, climbableLayer);

        if (wallCollider != null)
        {
            ClimbableSurface surface = wallCollider.GetComponent<ClimbableSurface>();

            if (surface != null)
            {
                currentClimbSpeed = climbSpeed * surface.climbSpeedMultiplier;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (frontCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(frontCheck.position, wallCheckRadius);
        }

        if (backCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(backCheck.position, wallCheckRadius);
        }
    }
}
