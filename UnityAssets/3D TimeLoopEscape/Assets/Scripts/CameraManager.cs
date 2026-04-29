using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [Header("Cameras")]
    public GameObject thirdPersonCamera;
    public GameObject firstPersonCamera;

    [Header("UI Elements")]
    public GameObject crosshairUI; // Drag your Crosshair Image here

    private bool isFirstPerson = false;

    void Start()
    {
        // Default startup view (Third-Person)
        thirdPersonCamera.SetActive(true);
        firstPersonCamera.SetActive(false);
        crosshairUI.SetActive(false); // Hide crosshair in TPC
    }

    void Update()
    {
        // Check for switch view input (V key)
        if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
        {
            isFirstPerson = !isFirstPerson;

            if (isFirstPerson)
            {
                // Switch to First-Person View
                firstPersonCamera.SetActive(true);
                thirdPersonCamera.SetActive(false);
                crosshairUI.SetActive(true); // Show crosshair for better aiming
                Debug.Log("Switched to First-Person Mode");
            }
            else
            {
                // Switch back to Third-Person View
                firstPersonCamera.SetActive(false);
                thirdPersonCamera.SetActive(true);
                crosshairUI.SetActive(false); // Hide crosshair
                Debug.Log("Switched to Third-Person Mode");
            }
        }
    }
}