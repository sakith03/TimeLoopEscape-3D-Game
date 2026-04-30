using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    
    public CharacterController controller; 
    
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 250f;
    public Animator animator;

    void Update()
    {
        float v = Input.GetAxis("Vertical");   // W/S
        float h = Input.GetAxis("Horizontal"); // A/D

        
        transform.Rotate(0, h * rotationSpeed * Time.deltaTime, 0);

        
        Vector3 moveDirection = transform.forward * v;

        
        if (controller != null)
        {
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        // ANIMATION
        animator.SetBool("isWalking", Mathf.Abs(v) > 0.1f);
    }
}