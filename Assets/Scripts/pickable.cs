
using UnityEngine;

public class pickable : MonoBehaviour, IInteractable
{
    private bool isHeld = false;
    private Transform holder;
    private Rigidbody rb;
    [SerializeField] private Transform handHoldPoint;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Interact()  // Changed: removed Transform parameter
    {
        if (!isHeld)
        {
            if (handHoldPoint == null)
            {
                Debug.LogWarning("Hand Hold Point is not assigned.");
                return;
            }
            holder = handHoldPoint;
            transform.SetParent(holder);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            rb.isKinematic = true;
            rb.detectCollisions = false;
            isHeld = true;
            Debug.Log("Object picked up");
        }
        else
        {
            transform.SetParent(null);
            rb.isKinematic = false;
            rb.detectCollisions = true;

            // Since we don't have the interactor parameter anymore, 
            // we'll find the camera to get the forward direction
            Camera playerCamera = Camera.main;
            if (playerCamera != null)
            {
                rb.velocity = playerCamera.transform.forward * 2f;
            }
            else
            {
                // Fallback: just drop without throwing
                rb.velocity = Vector3.zero;
            }

            isHeld = false;
            holder = null;
            Debug.Log("Object dropped");
        }
    }
}