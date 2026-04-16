using UnityEngine;
using UnityEngine.InputSystem; 
public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 50f; 
    
    float xRotation = 0f;
    float yRotation = 0f;

    void Start()
    {
        
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        
        if (Mouse.current != null)
        {
            
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            
            float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
            float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

            
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); 
            
            yRotation += mouseX;

            
            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
    }
}