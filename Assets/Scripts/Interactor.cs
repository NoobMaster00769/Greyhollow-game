using UnityEngine;
using UnityEngine.UI;

public interface IInteractable
{
    public void Interact();
}

public class Interactor : MonoBehaviour
{
    [SerializeField] private Image reticleImage;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color highlightColor = Color.red;
    [SerializeField] private Color heldColor = Color.green;
    [SerializeField] private Color climbingColor = Color.yellow; // New color for climbing state
    public Transform InteractorSource;
    public float InteractRange;
    [SerializeField] private float sphereRadius = 0.5f;

    private IInteractable heldObject;
    private PlayerMovement playerMovement; // Reference to check climbing state
    private IInteractable currentLadder; // Track current ladder we can interact with

    void Start()
    {
        // Get player movement component
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
        }
    }

    void Update()
    {
        // FIX: Check if player is currently climbing
        bool isClimbing = playerMovement != null && playerMovement.IsClimbing();

        if (isClimbing)
        {
            // Player is climbing - show climbing color and allow exit
            reticleImage.color = climbingColor;
            if (Input.GetKeyDown(KeyCode.E) && currentLadder != null)
            {
                Debug.Log("Exiting ladder");
                currentLadder.Interact(); // This will stop climbing
                currentLadder = null;
            }
            return;
        }

        // If we're holding something (non-ladder), pressing E should drop it
        if (heldObject != null)
        {
            reticleImage.color = heldColor;
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Dropping held object");
                heldObject.Interact();
                heldObject = null;
            }
            return;
        }

        // Normal interaction detection
        if (Physics.SphereCast(InteractorSource.position, sphereRadius, InteractorSource.forward, out RaycastHit hitInfo, InteractRange))
        {
            if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactobj))
            {
                reticleImage.color = highlightColor;
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // Check if this is a ladder
                    if (interactobj is Ladder)
                    {
                        Debug.Log($"Starting to climb: {hitInfo.collider.gameObject.name}");
                        currentLadder = interactobj;
                        interactobj.Interact();
                    }
                    else
                    {
                        Debug.Log($"Picking up: {hitInfo.collider.gameObject.name}");
                        interactobj.Interact();
                        heldObject = interactobj;
                    }
                }
            }
            else
            {
                reticleImage.color = defaultColor;
            }
        }
        else
        {
            reticleImage.color = defaultColor;
        }
    }
}