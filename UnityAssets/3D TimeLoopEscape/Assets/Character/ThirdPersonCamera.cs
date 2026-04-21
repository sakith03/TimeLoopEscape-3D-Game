using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    public float distance = 4f;
    public float height = 2f;

    public float mouseSensitivity = 3f;

    private float yaw = 0f;
    private float pitch = 10f;

    void LateUpdate()
    {
        // Mouse input
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, -10f, 60f);

        // Camera rotation around player
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 direction = new Vector3(0, 0, -distance);
        Vector3 position = target.position + rotation * direction;

        position.y += height;

        transform.position = position;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}