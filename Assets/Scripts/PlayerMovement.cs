using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 12f;
    public float sprintSpeed = 18f;
    public float strafeSpeed = 8f;
    public float backwardSpeed = 6f;
    public float gravity = -9.81f;
    Vector3 velocity;
    public Transform GroundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    bool isGrounded;
    public float JumpHeight = 3f;

    public Animator animator;

    public float maxSprintTime = 3f;
    public float sprintCooldown = 2f;
    private float currentSprintTime;
    private float sprintCooldownTimer;
    private bool isSprinting = false;
    private bool canSprint = true;
    private bool isJumping = false;

    // Falling system variables
    public float fallThreshold = -5f;
    public float coyoteTime = 0.2f;
    private bool isFalling = false;
    private bool wasGroundedLastFrame = false;
    private float timeOffGround = 0f;
    private bool justLanded = false;

    [Header("Climbing")]
    private bool isClimbing = false;
    private Ladder currentLadder;

    public bool IsClimbing()
    {
        return isClimbing;
    }

    public void SetClimbingState(bool climbing, Ladder ladder)
    {
        isClimbing = climbing;
        currentLadder = ladder;

        if (isClimbing)
        {
            // Reset velocity when starting to climb
            velocity = Vector3.zero;
            Debug.Log("Started climbing - disabled normal movement");
        }
        else
        {
            Debug.Log("Stopped climbing - enabled normal movement");
        }
    }

    private void Start()
    {
        currentSprintTime = maxSprintTime;
        wasGroundedLastFrame = true;
    }

    void Update()
    {
        // Handle climbing movement first - if climbing, skip normal movement
        if (isClimbing && currentLadder != null)
        {
            currentLadder.HandleClimbingMovement();
            return; // Exit early, don't process normal movement
        }

        // Normal movement code (unchanged)
        bool previousGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(GroundCheck.position, groundDistance, groundMask);

        // Handle ground state transitions
        if (isGrounded && !previousGrounded)
        {
            justLanded = true;
            isFalling = false;
            timeOffGround = 0f;
        }
        else if (!isGrounded && previousGrounded)
        {
            timeOffGround = 0f;
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Track time off ground for coyote time
        if (!isGrounded)
        {
            timeOffGround += Time.deltaTime;
        }

        // Determine if we're falling
        if (!isGrounded && !isJumping && velocity.y < fallThreshold && timeOffGround > coyoteTime)
        {
            isFalling = true;
        }
        else if (isGrounded || isJumping)
        {
            isFalling = false;
        }

        if (!canSprint)
        {
            sprintCooldownTimer -= Time.deltaTime;
            if (sprintCooldownTimer <= 0f)
            {
                canSprint = true;
                currentSprintTime = maxSprintTime;
            }
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        bool sprintInput = Input.GetKey(KeyCode.LeftShift);
        bool canSprintDirection = z > 0;

        // Handle sprinting
        if (sprintInput && currentSprintTime > 0f && canSprint && canSprintDirection)
        {
            isSprinting = true;
            currentSprintTime -= Time.deltaTime;
            if (currentSprintTime <= 0f)
            {
                isSprinting = false;
                canSprint = false;
                sprintCooldownTimer = sprintCooldown;
            }
        }
        else
        {
            isSprinting = false;
            if (canSprint && currentSprintTime < maxSprintTime)
            {
                currentSprintTime += Time.deltaTime * 0.5f;
                currentSprintTime = Mathf.Min(currentSprintTime, maxSprintTime);
            }
        }

        // Determine movement speed based on direction
        float currentSpeed = speed;

        bool isStrafeLeft = (x < -0.1f && Mathf.Abs(z) < 0.1f);
        bool isStrafeRight = (x > 0.1f && Mathf.Abs(z) < 0.1f);
        bool isMovingBackward = (z < -0.1f);

        if (isSprinting && canSprintDirection)
        {
            currentSpeed = sprintSpeed;
        }
        else if (isStrafeLeft || isStrafeRight)
        {
            currentSpeed = strafeSpeed;
        }
        else if (isMovingBackward)
        {
            currentSpeed = backwardSpeed;
        }
        else if (z > 0.1f)
        {
            currentSpeed = speed;
        }
        else if (Mathf.Abs(x) > 0.1f && Mathf.Abs(z) > 0.1f)
        {
            if (z > 0)
            {
                currentSpeed = isSprinting ? sprintSpeed : speed;
            }
            else
            {
                currentSpeed = backwardSpeed;
            }
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(JumpHeight * -2 * gravity);
            isJumping = true;
        }

        // Reset jumping state when we land
        if (isGrounded && velocity.y <= 0)
        {
            isJumping = false;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Update normal movement animations (only when not climbing)
        UpdateMovementAnimations(x, z);

        // Reset justLanded flag after one frame
        if (justLanded)
        {
            StartCoroutine(ResetJustLanded());
        }

        wasGroundedLastFrame = isGrounded;
    }

    private void UpdateMovementAnimations(float x, float z)
    {
        if (animator == null || isClimbing) return; // Don't update movement animations while climbing

        // Calculate animation parameters
        bool isMoving = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f);

        float animatorSpeed = 0f;
        float animatorStrafe = 0f;

        bool isStrafeLeft = (x < -0.1f && Mathf.Abs(z) < 0.1f);
        bool isStrafeRight = (x > 0.1f && Mathf.Abs(z) < 0.1f);

        if (isMoving && isGrounded)
        {
            if (isStrafeLeft)
            {
                animatorSpeed = 0f;
                animatorStrafe = -1f;
            }
            else if (isStrafeRight)
            {
                animatorSpeed = 0f;
                animatorStrafe = 1f;
            }
            else
            {
                animatorStrafe = 0f;

                if (isSprinting)
                {
                    animatorSpeed = 1f;
                }
                else if (z > 0)
                {
                    animatorSpeed = 0.5f;
                }
                else if (z < 0)
                {
                    animatorSpeed = -0.5f;
                }
            }
        }
        else
        {
            animatorSpeed = 0f;
            animatorStrafe = 0f;
        }

        // Set movement animation parameters
        animator.SetFloat("Speed", animatorSpeed, 0.1f, Time.deltaTime);
        animator.SetFloat("Strafe", animatorStrafe, 0.1f, Time.deltaTime);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isFalling", isFalling);
        animator.SetBool("justLanded", justLanded);
    }

    private IEnumerator ResetJustLanded()
    {
        yield return null;
        justLanded = false;
    }
}