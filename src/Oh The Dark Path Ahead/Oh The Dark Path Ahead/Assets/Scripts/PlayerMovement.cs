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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = visuals.localScale;
        state = MovementState.Normal;
    }

    void Update()
    {
        Debug.Log("Vel: " + rb.velocity);

        rb.gravityScale = (state == MovementState.Climbing) ? 0f : 4f;

        moveInput = Input.GetAxisRaw("Horizontal");
        climbInput = Input.GetAxisRaw("Vertical");

        grounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        wallFront = Physics2D.OverlapCircle(frontCheck.position, checkRadius, wallLayer);
        wallBack = Physics2D.OverlapCircle(backCheck.position, checkRadius, wallLayer);
        wall = wallFront || wallBack;

        bool headClear = !Physics2D.OverlapCircle(ledgeCheck.position, checkRadius, groundLayer);
        bool ledgeClear = !Physics2D.OverlapCircle(topCheck.position, checkRadius, groundLayer);

        bool canLedge =
            wall &&
            !grounded &&
            rb.velocity.y <= 0f &&
            headClear &&
            ledgeClear &&
            state != MovementState.LedgeHold &&
            state != MovementState.LedgeClimb;

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
                state = MovementState.Normal;
                rb.gravityScale = 4f;
            }

            return;
        }

        // ================= CLIMB TOGGLE =================
        if (Input.GetKeyDown(KeyCode.E) && wall && !grounded)
        {
            state = (state == MovementState.Climbing)
                ? MovementState.Normal
                : MovementState.Climbing;
        }

        // ================= CLIMB =================
        if (state == MovementState.Climbing)
        {
            // Disable gravity while climbing
            rb.gravityScale = 0f;

            // ONLY vertical movement
            rb.velocity = new Vector2(rb.velocity.x, climbInput * climbSpeed);

            // If no input → start sliding
            if (Mathf.Abs(climbInput) < 0.1f)
            {
                state = MovementState.WallSliding;
            }

            // If we lose the wall → fall
            if (!wall)
            {
                state = MovementState.Normal;
                rb.gravityScale = 4f;
            }

            return;
        }

        // ================= WALL SLIDE =================
        if (wall && !grounded && rb.velocity.y < 0 && state == MovementState.Normal)
        {
            state = MovementState.WallSliding;
        }

        if (state == MovementState.WallSliding)
        {
            rb.gravityScale = 4f;

            // Let gravity do the work, just clamp fall speed
            if (rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            }

            // Wall jump
            if (Input.GetButtonDown("Jump"))
            {
                state = MovementState.Normal;

                float dir = wallFront ? -1 : 1;
                rb.velocity = new Vector2(dir * wallJumpForce, jumpForce);
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
            float targetX = moveInput * moveSpeed;

            rb.velocity = new Vector2(targetX, rb.velocity.y);

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