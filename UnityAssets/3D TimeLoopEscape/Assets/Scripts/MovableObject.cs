using UnityEngine;

public class MovableObject : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("How far the object should move in X, Y, or Z direction")]
    public Vector3 moveDistance = new Vector3(2f, 0f, 0f); 
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private bool isMoved = false;

    void Start()
    {
        
        originalPosition = transform.position;
        
        targetPosition = originalPosition + moveDistance;
    }

   
    public void ToggleMove()
    {
        if (!isMoved)
        {
            
            transform.position = targetPosition;
            isMoved = true;
        }
        else
        {
            
            transform.position = originalPosition;
            isMoved = false;
        }
    }
}