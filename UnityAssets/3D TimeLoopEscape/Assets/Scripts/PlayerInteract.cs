using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerInteract : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera playerCamera;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                // 1. Check if it's a Door
                DoorController door = hit.collider.GetComponent<DoorController>();
                
                // 2. Check if it's a Movable Object (Chair/Bed)
                MovableObject movableItem = hit.collider.GetComponent<MovableObject>();

                if (door != null)
                {
                    door.ToggleDoor();
                    Debug.Log("Interacted with Door: " + hit.collider.gameObject.name);
                }
                else if (movableItem != null)
                {
                    movableItem.ToggleMove();
                    Debug.Log("Moved Object: " + hit.collider.gameObject.name);
                }
            }
        }
    }
}