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
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            
            Camera activeCamera = null;
            if (firstPersonCam != null && firstPersonCam.gameObject.activeInHierarchy)
                activeCamera = firstPersonCam;
            else if (thirdPersonCam != null && thirdPersonCam.gameObject.activeInHierarchy)
                activeCamera = thirdPersonCam;

            
            if (activeCamera == null) return; 

            Ray ray = new Ray(activeCamera.transform.position, activeCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactRange))
            {
                DoorController door = hit.collider.GetComponent<DoorController>();
                MovableObject movableItem = hit.collider.GetComponent<MovableObject>();

                if (door != null)
                {
                    door.ToggleDoor();
                }
                else if (movableItem != null)
                {
                    movableItem.ToggleMove();
                }
            }
        }
    }
}