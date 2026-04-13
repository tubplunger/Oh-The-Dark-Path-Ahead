using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAI : MonoBehaviour
{
    private enum AIState
    {
        Idle,
        Patrol,
        Chase,
        Jump
    }

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float chaseSpeed = 4f;
    public float jumpForce = 10f;

    [Header("Timing")]
    public float idleTime = 1.5f;
    public float patrolTime = 3f;
    public float jumpCooldown = 1f;

    [Header("Detection")]
    public Transform playerCheck;
    public float playerDetectRadius = 6f;
    public LayerMask playerLayer;

    [Header("Environment Checks")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public Transform wallCheck;
    public float wallCheckRadius = 0.2f;

    public Transform ledgeCheck;
    public float ledgeCheckRadius = 0.2f;

    [Header("Target")]
    public Transform playerTarget;

    private Rigidbody2D rb;
    private AIState currentState = AIState.Patrol;

    private bool isGrounded;
    private bool isWallAhead;
    private bool isLedgeAhead;
    private bool playerDetected;

    private float stateTimer;
    private float jumpCooldownTimer;

    private int moveDirection = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stateTimer = patrolTime;
    }

    private void Update()
    {
        UpdateChecks();
        UpdateTimers();
        UpdatePlayerDetection();
        UpdateStateMachine();
        UpdateFacing();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void UpdateChecks()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isWallAhead = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, groundLayer);
        isLedgeAhead = Physics2D.OverlapCircle(ledgeCheck.position, ledgeCheckRadius, groundLayer);
    }

    private void UpdateTimers()
    {
        if (stateTimer > 0f)
            stateTimer -= Time.deltaTime;

        if (jumpCooldownTimer > 0f)
            jumpCooldownTimer -= Time.deltaTime;
    }

    private void UpdatePlayerDetection()
    {
        Collider2D hit = Physics2D.OverlapCircle(playerCheck.position, playerDetectRadius, playerLayer);

        playerDetected = hit != null;

        if (playerDetected && hit != null)
        {
            playerTarget = hit.transform.root;
        }
        else if (!playerDetected)
        {
            playerTarget = null;
        }
    }

    private void UpdateStateMachine()
    {
        if (playerDetected && playerTarget != null)
        {
            currentState = AIState.Chase;
            return;
        }

        switch (currentState)
        {
            case AIState.Idle:
                if (stateTimer <= 0f)
                {
                    currentState = AIState.Patrol;
                    stateTimer = patrolTime;
                }
                break;

            case AIState.Patrol:
                if (stateTimer <= 0f)
                {
                    currentState = AIState.Idle;
                    stateTimer = idleTime;
                    rb.velocity = new Vector2(0f, rb.velocity.y);
                }
                break;

            case AIState.Chase:
                if (!playerDetected)
                {
                    currentState = AIState.Patrol;
                    stateTimer = patrolTime;
                }
                break;
        }

        if ((currentState == AIState.Patrol || currentState == AIState.Chase) && isGrounded)
        {
            if (ShouldTurnAround())
            {
                FlipDirection();
            }

            if (ShouldJump())
            {
                DoJump();
            }
        }
    }

    private void HandleMovement()
    {
        switch (currentState)
        {
            case AIState.Idle:
                rb.velocity = new Vector2(0f, rb.velocity.y);
                break;

            case AIState.Patrol:
                rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
                break;

            case AIState.Chase:
                if (playerTarget == null)
                    return;

                float directionToPlayer = Mathf.Sign(playerTarget.position.x - transform.position.x);

                if (Mathf.Abs(playerTarget.position.x - transform.position.x) > 0.2f)
                {
                    moveDirection = directionToPlayer > 0 ? 1 : -1;
                }

                rb.velocity = new Vector2(moveDirection * chaseSpeed, rb.velocity.y);
                break;
        }
    }

    private bool ShouldTurnAround()
    {
        if (currentState == AIState.Chase && playerTarget != null)
        {
            float playerDir = Mathf.Sign(playerTarget.position.x - transform.position.x);
            bool playerIsBehind = (playerDir > 0 && moveDirection < 0) || (playerDir < 0 && moveDirection > 0);

            if (playerIsBehind)
                return true;
        }

        if (isWallAhead)
            return true;

        if (!isLedgeAhead)
            return true;

        return false;
    }

    private bool ShouldJump()
    {
        if (jumpCooldownTimer > 0f)
            return false;

        if (!isGrounded)
            return false;

        if (!isWallAhead)
            return false;

        if (currentState == AIState.Chase && playerTarget != null)
        {
            bool playerHigher = playerTarget.position.y > transform.position.y - 0.2f;
            return playerHigher;
        }

        return false;
    }

    private void DoJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpCooldownTimer = jumpCooldown;
    }

    private void FlipDirection()
    {
        moveDirection *= -1;
    }

    private void UpdateFacing()
    {
        if (moveDirection == 0)
            return;

        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x) * moveDirection,
            transform.localScale.y,
            transform.localScale.z
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }

        if (ledgeCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(ledgeCheck.position, ledgeCheckRadius);
        }

        if (playerCheck != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(playerCheck.position, playerDetectRadius);
        }
    }
}

