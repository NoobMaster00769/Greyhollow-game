// File: PlayerLook.cs
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float mouseSensitivity = 1f;

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Implement rotation based on your camera/player setup
    }
}
