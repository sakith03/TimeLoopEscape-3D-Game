using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 250f;
    public Animator animator;

    void Update()
    {
        float v = Input.GetAxis("Vertical");   // W/S
        float h = Input.GetAxis("Horizontal"); // A/D

        // ROTATE CHARACTER (A/D ONLY)
        transform.Rotate(0, h * rotationSpeed * Time.deltaTime, 0);

        // MOVE FORWARD/BACKWARD
        Vector3 move = transform.forward * v;
        transform.position += move * moveSpeed * Time.deltaTime;

        // ANIMATION
        animator.SetBool("isWalking", Mathf.Abs(v) > 0.1f);
    }
}