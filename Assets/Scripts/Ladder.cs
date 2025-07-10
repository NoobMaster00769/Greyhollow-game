using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour, IInteractable
{
    [Header("Ladder Settings")]
    public Transform climbBottom;
    public Transform climbTop;
    public float climbSpeed = 5f;
    public float climbDetectionRange = 2f;
    [Header("Positioning")]
    public Vector3 climbOffset = Vector3.zero;
    public bool followLadderAngle = true;
    public float distanceFromLadder = 0.5f;

    private PlayerMovement playerMovement;
    private bool isPlayerClimbing = false;
    private Transform player;
    private CharacterController playerController;

    public void Interact()
    {
        if (playerMovement == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                playerObj = FindObjectOfType<PlayerMovement>()?.gameObject;
            }

            if (playerObj != null)
            {
                playerMovement = playerObj.GetComponent<PlayerMovement>();
                playerController = playerObj.GetComponent<CharacterController>();
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("Could not find PlayerMovement component!");
                return;
            }
        }

        if (!isPlayerClimbing)
        {
            StartClimbing();
        }
        else
        {
            StopClimbing();
        }
    }

    private void StartClimbing()
    {
        if (playerMovement != null)
        {
            // Disable the CharacterController temporarily to position the player
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            // Keep the player's current Y position when starting climbing
            float startY = player.position.y;

            // Calculate proper climbing position at current height
            Vector3 climbingPosition = GetClimbingPosition(startY);

            Debug.Log($"Start climbing - Player Y: {startY}, Climbing pos: {climbingPosition}");

            player.position = climbingPosition;

            // Align player rotation to face the ladder properly
            Vector3 ladderDirection = GetLadderDirectionAtHeight(startY);
            if (ladderDirection != Vector3.zero)
            {
                // Face towards the ladder
                player.rotation = Quaternion.LookRotation(-ladderDirection);
            }

            // Re-enable the CharacterController
            if (playerController != null)
            {
                playerController.enabled = true;
            }

            // Set climbing state
            isPlayerClimbing = true;
            playerMovement.SetClimbingState(true, this);

            Debug.Log("Started climbing ladder");
        }
    }

    private Vector3 GetClimbingPosition(float currentY)
    {
        if (!followLadderAngle || climbBottom == null || climbTop == null)
        {
            Vector3 basePosition = transform.position + climbOffset;
            basePosition.y = currentY;
            return basePosition;
        }

        float climbProgress = Mathf.InverseLerp(climbBottom.position.y, climbTop.position.y, currentY);
        climbProgress = Mathf.Clamp01(climbProgress);

        Vector3 bottomXZ = new Vector3(climbBottom.position.x, 0, climbBottom.position.z);
        Vector3 topXZ = new Vector3(climbTop.position.x, 0, climbTop.position.z);
        Vector3 ladderPointXZ = Vector3.Lerp(bottomXZ, topXZ, climbProgress);

        Vector3 ladderDirection = GetLadderDirectionAtHeight(currentY);

        Vector3 climbingPosition = ladderPointXZ + (ladderDirection * distanceFromLadder) + climbOffset;
        climbingPosition.y = currentY;

        return climbingPosition;
    }

    private Vector3 GetLadderDirectionAtHeight(float height)
    {
        if (climbBottom == null || climbTop == null) return transform.forward;

        Vector3 ladderUp = (climbTop.position - climbBottom.position).normalized;
        Vector3 worldUp = Vector3.up;
        Vector3 ladderRight = Vector3.Cross(ladderUp, worldUp).normalized;
        Vector3 ladderForward = Vector3.Cross(ladderRight, ladderUp).normalized;

        return ladderForward;
    }

    public void StopClimbing()
    {
        if (playerMovement != null && isPlayerClimbing)
        {
            isPlayerClimbing = false;
            playerMovement.SetClimbingState(false, null);

            Debug.Log("Stopped climbing ladder");
        }
    }

    public void HandleClimbingMovement()
    {
        if (!isPlayerClimbing || player == null) return;

        float verticalInput = Input.GetAxis("Vertical");

        float currentY = player.position.y;
        float newY = currentY + (verticalInput * climbSpeed * Time.deltaTime);

        float bottomY = climbBottom.position.y;
        float topY = climbTop.position.y;

        // Check if player reached bottom
        if (newY <= bottomY && verticalInput < 0)
        {
            // Player reached bottom - stop climbing
            StopClimbing();
            return;
        }

        // Check if player reached top (for now just stop climbing, we'll add special exit later)
        if (newY >= topY && verticalInput > 0)
        {
            // For now, just stop climbing at the top
            StopClimbing();
            return;
        }

        // Clamp Y position to ladder bounds
        newY = Mathf.Clamp(newY, bottomY, topY);

        // Only move if there's actual movement
        if (Mathf.Abs(newY - currentY) > 0.001f)
        {
            Vector3 newPosition = GetClimbingPosition(newY);

            // Use CharacterController.Move for smooth movement
            if (playerController != null && playerController.enabled)
            {
                Vector3 movement = newPosition - player.position;
                playerController.Move(movement);
            }
            else
            {
                player.position = newPosition;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (climbBottom != null && climbTop != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(climbBottom.position, climbTop.position);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(climbBottom.position, 0.2f);
            Gizmos.DrawWireSphere(climbTop.position, 0.2f);

            if (followLadderAngle)
            {
                Gizmos.color = Color.red;
                int steps = 10;
                for (int i = 0; i <= steps; i++)
                {
                    float progress = (float)i / steps;
                    float height = Mathf.Lerp(climbBottom.position.y, climbTop.position.y, progress);
                    Vector3 climbPos = GetClimbingPosition(height);
                    Gizmos.DrawWireCube(climbPos, Vector3.one * 0.3f);

                    if (i > 0)
                    {
                        float prevHeight = Mathf.Lerp(climbBottom.position.y, climbTop.position.y, (float)(i - 1) / steps);
                        Vector3 prevPos = GetClimbingPosition(prevHeight);
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawLine(prevPos, climbPos);
                    }
                }
            }
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, climbDetectionRange);

        if (climbBottom != null && climbTop != null)
        {
            Vector3 ladderDirection = GetLadderDirectionAtHeight(transform.position.y);
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, ladderDirection * 2f);
        }
    }
}