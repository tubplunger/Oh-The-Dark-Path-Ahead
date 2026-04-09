using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private enum MovementState
    {
        Normal,
        Climbing,
        WallSliding,
        LedgeHold,
        LedgeClimb
    }

    private MovementState state;

    [Header("Visuals")]
    public Transform visuals;
    private Vector3 originalScale;

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float climbSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 12f;
    public float wallJumpForce = 10f;
    public float wallJumpHorizontalForce = 12f;
    public float wallJumpVerticalForce = 10f;

    [Header("Wall Slide")]
    public float wallSlideSpeed = 2f;

    [Header("Ledge")]
    public float ledgeClimbHeight = 1.5f;
    public float ledgeClimbDuration = 0.25f;

    [Header("Checks")]
    public Transform groundCheck;
    public Transform frontCheck;
    public Transform backCheck;
    public Transform ledgeCheck;
    public Transform topCheck;

    public float checkRadius = 0.2f;

    public LayerMask groundLayer;
    public LayerMask wallLayer;

    private Rigidbody2D rb;

    private float moveInput;
    private float climbInput;

    private bool grounded;
    private bool wallFront;
    private bool wallBack;
    private bool wall;

    private Vector3 ledgeStart;
    private Vector3 ledgeEnd;
    private float ledgeT;
    private float ledgeCooldown = 0.2f;
    private float ledgeCooldownTimer = 0f;

    private float climbStartBuffer = 0.15f;
    private float climbStartTimer = 0f;

    private float wallDetachTime = 0.1f;
    private float wallDetachTimer = 0f;
    private float wallJumpLockTime = 0.15f;
    private float wallJumpLockTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = visuals.localScale;
        state = MovementState.Normal;
    }

    void Update()
    {
        Debug.Log("State: " + state);

        if (ledgeCooldownTimer > 0)
        {
            ledgeCooldownTimer -= Time.deltaTime;
        }

        if (climbStartTimer > 0)
        {
            climbStartTimer -= Time.deltaTime;
        }

        if (wallDetachTimer > 0)
        {
            wallDetachTimer -= Time.deltaTime;
        }

        if (wallJumpLockTimer > 0)
        {
            wallJumpLockTimer -= Time.deltaTime;
        }

        moveInput = Input.GetAxisRaw("Horizontal");
        climbInput = Input.GetAxisRaw("Vertical");

        grounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        wallFront = Physics2D.OverlapCircle(frontCheck.position, checkRadius, wallLayer);
        wallBack = Physics2D.OverlapCircle(backCheck.position, checkRadius, wallLayer);
        wall = (wallFront || wallBack) && wallDetachTimer <= 0f && wallJumpLockTimer <= 0f;

        bool headClear = !Physics2D.OverlapCircle(ledgeCheck.position, checkRadius, groundLayer);
        bool ledgeClear = !Physics2D.OverlapCircle(topCheck.position, checkRadius, groundLayer);

        bool canLedge =
    ledgeCooldownTimer <= 0f &&
    state == MovementState.Normal &&
    wall &&
    !grounded &&
    rb.velocity.y < -2f &&
    climbInput >= 0f &&
    headClear &&
    ledgeClear;

        // ================= LEDGE =================
        if (canLedge)
        {
            state = MovementState.LedgeHold;
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
            return;
        }

        if (state == MovementState.LedgeHold)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                state = MovementState.LedgeClimb;
                ledgeT = 0f;

                ledgeStart = transform.position;
                ledgeEnd = transform.position + new Vector3(
                    -Mathf.Sign(visuals.localScale.x),
                    ledgeClimbHeight,
                    0f
                );
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                ledgeCooldownTimer = ledgeCooldown;
                state = MovementState.Normal;
                rb.gravityScale = 4f;
            }

            return;
        }

        if (state == MovementState.LedgeClimb)
        {
            ledgeT += Time.deltaTime / ledgeClimbDuration;
            transform.position = Vector3.Lerp(ledgeStart, ledgeEnd, ledgeT);

            if (ledgeT >= 1f)
            {
                ledgeCooldownTimer = ledgeCooldown;
                state = MovementState.Normal;
                rb.gravityScale = 4f;
            }

            return;
        }

        // ================= CLIMB TOGGLE =================
        if (Input.GetKeyDown(KeyCode.E) && wall)
        {
            if (state == MovementState.Climbing)
            {
                state = MovementState.Normal;
            }
            else
            {
                state = MovementState.Climbing;
                climbStartTimer = climbStartBuffer;
            }
        }

        // ================= CLIMB =================
        if (state == MovementState.Climbing)
        {
            rb.gravityScale = 0f;

            float vertical = climbInput;

            // If player JUST started climbing, give a small upward boost
            if (climbStartTimer > 0f && Mathf.Abs(climbInput) < 0.1f)
            {
                vertical = 0.5f; // auto-start climb
            }

            rb.velocity = new Vector2(
                rb.velocity.x,
                vertical * climbSpeed
            );

            if (Mathf.Abs(climbInput) < 0.1f && climbStartTimer <= 0f && rb.velocity.y <= 0f)
            {
                state = MovementState.WallSliding;
                rb.gravityScale = 4f;
                return;
            }

            if (!wall)
            {
                state = MovementState.Normal;
                rb.gravityScale = 4f;
            }

            if (Input.GetButtonDown("Jump"))
            {
                state = MovementState.Normal;

                float dir = wallFront ? -1 : 1;

                rb.velocity = new Vector2(
                    dir * wallJumpHorizontalForce,
                    wallJumpVerticalForce
                );

                rb.position += new Vector2(dir * 0.2f, 0f);

                wallDetachTimer = wallDetachTime;
                wallJumpLockTimer = wallJumpLockTime;

                rb.gravityScale = 4f;
                return;
            }

            return;
        }

        // ================= WALL SLIDE =================
        if (wall && !grounded && rb.velocity.y < -0.1f && state == MovementState.Normal)
        {
            state = MovementState.WallSliding;
        }

        if (state == MovementState.WallSliding)
        {
            rb.gravityScale = 4f;

            // ONLY limit fall speed
            if (rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(
                    rb.velocity.x, // KEEP X
                    -wallSlideSpeed
                );
            }

            // Wall jump
            if (Input.GetButtonDown("Jump"))
            {
                state = MovementState.Normal;

                float dir = wallFront ? -1 : 1;

                rb.velocity = new Vector2(
                    dir * wallJumpHorizontalForce,
                    wallJumpVerticalForce
                );

                rb.position += new Vector2(dir * 0.2f, 0f);

                wallDetachTimer = wallDetachTime;
                wallJumpLockTimer = wallJumpLockTime;

                rb.gravityScale = 4f;
                return;
            }

            // Climb again
            if (Mathf.Abs(climbInput) > 0.1f)
            {
                state = MovementState.Climbing;
                return;
            }

            // Leave wall
            if (!wall || grounded)
            {
                state = MovementState.Normal;
            }
        }

        // ================= NORMAL =================
        if (state == MovementState.Normal)
        {
            rb.gravityScale = 4f;

            if (wallJumpLockTimer <= 0f)
            {
                float targetX = moveInput * moveSpeed;
                rb.velocity = new Vector2(targetX, rb.velocity.y);
            }

            if (Input.GetButtonDown("Jump") && grounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        }

        // ================= FLIP =================
        if (moveInput != 0)
        {
            visuals.localScale = new Vector3(
                Mathf.Sign(moveInput) * Mathf.Abs(originalScale.x),
                originalScale.y,
                originalScale.z
            );
        }
    }
}