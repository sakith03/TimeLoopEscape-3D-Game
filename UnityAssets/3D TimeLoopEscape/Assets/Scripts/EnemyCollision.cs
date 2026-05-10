using UnityEngine;

public class EnemyCollision : MonoBehaviour
{
    public GameManager gameManager;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger hit: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("PLAYER HIT NPC → LOSE");
            gameManager.LoseGame();
        }
    }
}