using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerInteract : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("Attach the Main Camera here")]
    public Camera playerCamera;

    void Update()
    {
        // Check if the 'E' key was pressed this frame using the New Input System
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            // Create a ray from the camera's exact position, shooting straight forward
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            // Cast the ray to infinity to ensure it reaches the objects regardless of world scale
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                // Try to get the DoorController script from the object that the ray hit
                DoorController door = hit.collider.GetComponent<DoorController>();

                // If the script exists (meaning it is a functional door), toggle its state
                if (door != null)
                {
                    door.ToggleDoor();
                    Debug.Log("Successfully interacted with: " + hit.collider.gameObject.name);
                }
            }
        }
    }
}