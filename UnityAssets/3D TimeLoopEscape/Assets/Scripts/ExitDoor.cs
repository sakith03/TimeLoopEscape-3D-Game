using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public GameManager gameManager;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered trigger: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("PLAYER WON");

            gameManager.WinGame();
        }
    }
}