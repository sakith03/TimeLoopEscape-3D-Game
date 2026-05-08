using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("Camera References")]
    public Camera thirdPersonCam;
    public Camera firstPersonCam;

    [Header("Interaction Settings")]
    public float interactRange = 4f;

    void Update()
    {
        // Check if the 'E' key was pressed this frame
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            // Find the currently active camera
            Camera activeCamera = null;

            if (firstPersonCam != null && firstPersonCam.gameObject.activeInHierarchy)
                activeCamera = firstPersonCam;
            else if (thirdPersonCam != null && thirdPersonCam.gameObject.activeInHierarchy)
                activeCamera = thirdPersonCam;

            // Stop if no active camera is found
            if (activeCamera == null) return;

            // Create a ray from the center of the screen
            // This works correctly for both first-person and third-person crosshair interactions
            Ray ray = activeCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            RaycastHit hit;

            // Check if the ray hits an object within interaction range
            if (Physics.Raycast(ray, out hit, interactRange))
            {
                // Try to find a DoorController component on the hit object
                // or its parent object
                DoorController door = hit.collider.GetComponentInParent<DoorController>();

                if (door == null)
                    door = hit.collider.GetComponent<DoorController>();

                // Check if the object has a MovableObject component
                MovableObject movableItem = hit.collider.GetComponent<MovableObject>();

                // Open/close the door if a door was hit
                if (door != null)
                {
                    door.ToggleDoor();
                }
                // Move/unmove the object if a movable object was hit
                else if (movableItem != null)
                {
                    movableItem.ToggleMove();
                }
            }
        }
    }
}