using UnityEngine;
using UnityEngine.InputSystem;

public class FPMouseLook : MonoBehaviour
{
    [Header("Sensitivity Settings")]
    [Tooltip("Adjust this IN THE INSPECTOR. Try values between 0.1 and 2.0")]
    public float mouseSensitivity = 0.5f; // Default eka godak adu kala

    [Header("References")]
    public Transform playerBody; 

    private float xRotation = 0f;

    void Start()
    {
        // Lock the cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Get raw mouse movement using the New Input System
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            
            // WADAGATH: Removed Time.deltaTime for raw, accurate 1:1 mouse movement
            float mouseX = mouseDelta.x * mouseSensitivity;
            float mouseY = mouseDelta.y * mouseSensitivity;

            // Handle vertical rotation (Looking up and down)
            xRotation -= mouseY;
            // Clamp rotation to prevent looking too far up or down (neck snapping)
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            // Apply vertical rotation to the camera locally
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // Handle horizontal rotation (Turning the entire player body left and right)
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}