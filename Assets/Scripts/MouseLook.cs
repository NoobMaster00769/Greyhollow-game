using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float mouseSens = 100f;
    public Transform Iris; // Player body

    [Header("Normal Camera Limits")]
    public float minLookAngle = -60f;
    public float maxLookAngle = 70f;

    [Header("Climbing Camera Limits")]
    public float climbingMinLookAngle = -20f; // Prevent looking down at legs
    public float climbingMaxLookAngle = 80f;
    public float climbingMinHorizontalAngle = -45f; // Limit horizontal look while climbing
    public float climbingMaxHorizontalAngle = 45f;

    [Header("Climbing Bobbing")]
    public float climbingBobbingAmount = 0.15f;
    public float climbingBobbingSpeed = 8f;
    public float climbingBobbingSmoothing = 0.1f;

    [Header("Animator Control")]
    public bool disableAnimatorWhileClimbing = true;

    private float xRotation = 0f;
    private float yRotation = 0f; // Track horizontal rotation for clamping
    private PlayerMovement playerMovement;
    private Animator characterAnimator;

    // Bobbing variables
    private Vector3 originalCameraPosition;
    private float bobbingTimer = 0f;
    private bool wasClimbing = false;
    private float currentBobbingOffset = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // Get player movement reference
        playerMovement = FindObjectOfType<PlayerMovement>();

        // Get animator reference - check multiple locations
        characterAnimator = GetComponent<Animator>();
        if (characterAnimator == null)
            characterAnimator = GetComponentInParent<Animator>();
        if (characterAnimator == null)
            characterAnimator = GetComponentInChildren<Animator>();

        if (characterAnimator != null)
        {
            Debug.Log($"Found Animator on: {characterAnimator.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("No Animator found for climbing control");
        }

        // Set the exact original camera position based on your transform values
        originalCameraPosition = new Vector3(0.009f, 0.15f, 0.14f);
        Debug.Log($"Set original camera position to: {originalCameraPosition}");

        // Initialize rotation tracking
        yRotation = Iris.eulerAngles.y;
    }

    void Update()
    {
        HandleMouseLook();
        HandleClimbingEffects();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSens * Time.deltaTime;

        bool isCurrentlyClimbing = playerMovement != null && playerMovement.IsClimbing();

        if (isCurrentlyClimbing)
        {
            // While climbing - clamp horizontal rotation
            yRotation += mouseX;
            yRotation = Mathf.Clamp(yRotation, climbingMinHorizontalAngle, climbingMaxHorizontalAngle);

            // Apply clamped horizontal rotation to body
            Iris.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
        else
        {
            // Normal movement - no horizontal clamping
            Iris.Rotate(Vector3.up * mouseX);
            // Update our tracking variable to match current rotation
            yRotation = Iris.eulerAngles.y;
            // Normalize angle to -180 to 180 range for consistent tracking
            if (yRotation > 180f) yRotation -= 360f;
        }

        // Vertical rotation with different limits based on climbing state
        xRotation -= mouseY;

        if (isCurrentlyClimbing)
        {
            xRotation = Mathf.Clamp(xRotation, climbingMinLookAngle, climbingMaxLookAngle);
        }
        else
        {
            xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);
        }

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleClimbingEffects()
    {
        bool isClimbing = playerMovement != null && playerMovement.IsClimbing();

        // Handle animator state changes
        if (isClimbing != wasClimbing)
        {
            if (isClimbing)
            {
                // Entering climbing state
                StartClimbing();
            }
            else
            {
                // Exiting climbing state
                StopClimbing();
            }
        }

        if (isClimbing)
        {
            // Check if player is actually moving while climbing
            float verticalInput = Input.GetAxis("Vertical");
            bool isMovingOnLadder = Mathf.Abs(verticalInput) > 0.1f;

            if (isMovingOnLadder)
            {
                // Update bobbing timer
                bobbingTimer += Time.deltaTime * climbingBobbingSpeed;

                // Calculate bobbing offset
                float targetBobbingOffset = Mathf.Sin(bobbingTimer) * climbingBobbingAmount;
                currentBobbingOffset = Mathf.Lerp(currentBobbingOffset, targetBobbingOffset, climbingBobbingSmoothing);

                // Apply camera bobbing
                Vector3 bobbingOffset = new Vector3(
                    Mathf.Sin(bobbingTimer * 0.7f) * climbingBobbingAmount * 0.3f, // Slight horizontal sway
                    currentBobbingOffset,
                    Mathf.Sin(bobbingTimer * 1.2f) * climbingBobbingAmount * 0.2f  // Slight forward/back movement
                );

                transform.localPosition = originalCameraPosition + bobbingOffset;
            }
            else
            {
                // Gradually stop bobbing when not moving
                currentBobbingOffset = Mathf.Lerp(currentBobbingOffset, 0f, climbingBobbingSmoothing * 2f);
                Vector3 bobbingOffset = new Vector3(0, currentBobbingOffset, 0);
                transform.localPosition = originalCameraPosition + bobbingOffset;
            }
        }

        wasClimbing = isClimbing;
    }

    void StartClimbing()
    {
        Debug.Log("Starting climbing - disabling animator and enabling horizontal clamping");

        // Disable animator to prevent walking animations
        if (characterAnimator != null && disableAnimatorWhileClimbing)
        {
            characterAnimator.enabled = false;
            Debug.Log("Animator disabled for climbing");
        }

        // Reset horizontal rotation tracking to current body rotation
        yRotation = Iris.eulerAngles.y;
        if (yRotation > 180f) yRotation -= 360f;

        // Clamp initial rotation if it's outside climbing limits
        yRotation = Mathf.Clamp(yRotation, climbingMinHorizontalAngle, climbingMaxHorizontalAngle);
        Iris.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    void StopClimbing()
    {
        Debug.Log("Stopping climbing - re-enabling animator and removing horizontal clamping");

        // Re-enable animator
        if (characterAnimator != null && disableAnimatorWhileClimbing)
        {
            characterAnimator.enabled = true;
            Debug.Log("Animator re-enabled after climbing");
        }

        // Reset camera position immediately
        transform.localPosition = originalCameraPosition;

        // Reset bobbing variables
        bobbingTimer = 0f;
        currentBobbingOffset = 0f;

        // Update rotation tracking to current body rotation for smooth transition
        yRotation = Iris.eulerAngles.y;
        if (yRotation > 180f) yRotation -= 360f;
    }

    // Public method to manually control animator
    public void SetAnimatorEnabled(bool enabled)
    {
        if (characterAnimator != null)
        {
            characterAnimator.enabled = enabled;
            Debug.Log($"Animator manually set to: {enabled}");
        }
    }

    // Public method to force reset everything
    public void ForceResetToOriginal()
    {
        transform.localPosition = originalCameraPosition;

        bobbingTimer = 0f;
        currentBobbingOffset = 0f;

        // Re-enable animator if it was disabled
        if (characterAnimator != null)
            characterAnimator.enabled = true;

        // Reset rotation tracking
        yRotation = Iris.eulerAngles.y;
        if (yRotation > 180f) yRotation -= 360f;

        Debug.Log("Forced reset to original positions and re-enabled animator");
    }

    // Debug visualization (optional - can be removed if not needed)
    void OnDrawGizmos()
    {
        // Draw gizmo at camera position to visualize it
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.1f);
    }
}