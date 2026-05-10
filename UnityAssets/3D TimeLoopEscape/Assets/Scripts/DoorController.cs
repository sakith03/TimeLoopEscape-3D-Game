using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Hinge Settings")]
    [Tooltip("Drag the DoorPivot object here")]
    public Transform hinge;

    [Header("Rotation Settings")]
    public float openAngle = 90f;
    public float smoothSpeed = 5f;

    [Tooltip("Set the axis to 1 for the direction it should open. Default is Y (0, 1, 0). Try X (1, 0, 0) or Z (0, 0, 1) if it opens like an oven!")]
    public Vector3 rotationAxis = new Vector3(0, 1, 0);

    [Header("IS2 Integration")]
    [Tooltip("Drag the DoorInteractable component from this same GameObject")]
    public DoorInteractable doorInteractable;

    private bool isOpen = false;
    private Quaternion defaultRotation;
    private Quaternion targetRotation;

    void Start()
    {
        if (hinge == null)
        {
            Debug.LogError("DOOR ERROR: Hinge slot is empty! Please assign the Pivot.");
            return;
        }

        defaultRotation = hinge.localRotation;
        targetRotation = defaultRotation;
    }

    void Update()
    {
        if (hinge != null)
        {
            hinge.localRotation = Quaternion.Slerp(
                hinge.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
        }
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            targetRotation = defaultRotation * Quaternion.Euler(rotationAxis * openAngle);

            // === IS2 HOOK — door opened, restore graph edge ===
            if (doorInteractable != null)
                doorInteractable.OpenDoor();
        }
        else
        {
            targetRotation = defaultRotation;

            // === IS2 HOOK — door closed, sever graph edge ===
            if (doorInteractable != null)
                doorInteractable.CloseDoor();
        }
    }
}