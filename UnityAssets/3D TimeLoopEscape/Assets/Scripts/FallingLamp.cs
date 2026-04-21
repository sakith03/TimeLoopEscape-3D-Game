using UnityEngine;

public class FallingLamp : MonoBehaviour
{
    [Header("Lamp Settings")]
    [Tooltip("Bittiye alawala thiyena lampuwe Rigidbody eka methanata drag karanna")]
    public Rigidbody lampRigidbody;

    
    void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player"))
        {
            
            lampRigidbody.isKinematic = false;

            Debug.Log("Lampuwa bima watuna!");

            
            Destroy(gameObject); 
        }
    }
}