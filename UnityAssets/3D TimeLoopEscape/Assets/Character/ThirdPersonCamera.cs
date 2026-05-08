using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;          // The player or object the camera follows
    public float distance = 4f;        // How far back the camera is from the target
    public float height = 2f;          // How high the camera is positioned

    [Header("Control Settings")]
    public float mouseSensitivity = 3f; // Speed of camera movement based on mouse

    private float yaw = 0f;            // Horizontal rotation (Left/Right)
    private float pitch = 10f;         // Vertical rotation (Up/Down)

    void Start()
    {
        // Locks the mouse cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
        
        // Makes the mouse cursor invisible during gameplay
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        // Check if a target is assigned to avoid errors
        if (target == null) return;

        // Get mouse input and multiply by sensitivity
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Limit the vertical rotation so the camera doesn't flip over
        pitch = Mathf.Clamp(pitch, -10f, 60f);

        // Convert the yaw and pitch into a Rotation (Quaternion)
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // Calculate the camera's position relative to the player
        Vector3 direction = new Vector3(0, 0, -distance);
        Vector3 position = target.position + rotation * direction;

        // Apply the height offset
        position.y += height;

        // Set the camera's position
        transform.position = position;
        
        // Point the camera at the target's center 
        // (1.5f offset is added to look at the player's chest/head instead of feet)
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}