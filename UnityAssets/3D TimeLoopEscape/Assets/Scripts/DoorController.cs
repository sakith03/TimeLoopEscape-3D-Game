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
    public Vector3 rotationAxis = new Vector3(0, 1, 0); // This lets you choose the axis in the Inspector

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

        // Store the starting rotation of the door
        defaultRotation = hinge.localRotation;
        targetRotation = defaultRotation;
    }

    void Update()
    {
        // Smoothly rotate the door towards the target rotation using Slerp
        if (hinge != null)
        {
            hinge.localRotation = Quaternion.Slerp(hinge.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
        }
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            // Multiply the chosen axis (X, Y, or Z) by the openAngle
            targetRotation = defaultRotation * Quaternion.Euler(rotationAxis * openAngle);
        }
        else
        {
            // Return to the default closed rotation
            targetRotation = defaultRotation;
        }
    }
}