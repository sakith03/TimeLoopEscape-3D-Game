using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Loop Settings")]
    public float loopDuration = 40f;
    private float timer;

    [Header("UI")]
    public TMP_Text timerText;

    [Header("Win UI")]
    public TMP_Text winText;

    [Header("Lose UI")]
    public TMP_Text loseText;

    [HideInInspector]
    public bool gameWon = false;

    [HideInInspector]
    public bool gameLost = false;

    [Header("Main Character")]
    public Transform mainCharacter;
    public Transform mainCharacterSpawn;

    [Header("NPC Ghost")]
    public Transform npcGhost;
    public Transform npcSpawn;

    private Vector3 characterStartPosition;
    private Quaternion characterStartRotation;

    private Vector3 npcStartPosition;
    private Quaternion npcStartRotation;

    private DoorController[] doors;

    void Start()
    {
        timer = loopDuration;

        // Hide UI at start
        if (winText != null) winText.enabled = false;
        if (loseText != null) loseText.enabled = false;

        // Player spawn
        if (mainCharacterSpawn != null)
        {
            characterStartPosition = mainCharacterSpawn.position;
            characterStartRotation = mainCharacterSpawn.rotation;
        }

        // NPC spawn
        if (npcSpawn != null)
        {
            npcStartPosition = npcSpawn.position;
            npcStartRotation = npcSpawn.rotation;
        }

        doors = FindObjectsOfType<DoorController>();

        UpdateTimerUI();
    }

    void Update()
    {
        if (gameWon || gameLost) return;

        timer -= Time.deltaTime;

        UpdateTimerUI();

        if (timer <= 0f)
        {
            ResetLoop();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = "Time Left: " + Mathf.Ceil(timer).ToString();
        }
    }

    // WIN
    public void WinGame()
    {
        if (gameLost) return;

        gameWon = true;

        if (winText != null)
        {
            winText.enabled = true;
            winText.text = "YOU WON";
        }

        Debug.Log("PLAYER WON");
    }

    // LOSE
    public void LoseGame()
    {
        if (gameWon) return;

        gameLost = true;

        if (loseText != null)
        {
            loseText.enabled = true;
            loseText.text = "YOU LOST";
        }

        Debug.Log("PLAYER LOST");
    }

    void ResetLoop()
    {
        timer = loopDuration;

        ResetMainCharacter();
        ResetNPC();
        ResetDoors();
    }

    void ResetMainCharacter()
    {
        if (mainCharacter == null) return;

        CharacterController cc = mainCharacter.GetComponent<CharacterController>();
        NavMeshAgent agent = mainCharacter.GetComponent<NavMeshAgent>();
        Rigidbody rb = mainCharacter.GetComponent<Rigidbody>();

        if (cc != null) cc.enabled = false;
        if (agent != null) agent.enabled = false;

        mainCharacter.position = characterStartPosition;
        mainCharacter.rotation = characterStartRotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (cc != null) cc.enabled = true;
        if (agent != null) agent.enabled = true;
    }

    void ResetNPC()
    {
        if (npcGhost == null) return;

        NavMeshAgent agent = npcGhost.GetComponent<NavMeshAgent>();
        Rigidbody rb = npcGhost.GetComponent<Rigidbody>();

        if (agent != null) agent.enabled = false;

        npcGhost.position = npcStartPosition;
        npcGhost.rotation = npcStartRotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (agent != null)
        {
            agent.ResetPath();
            agent.enabled = true;
        }
    }

    void ResetDoors()
    {
        foreach (DoorController door in doors)
        {
            if (door != null && door.hinge != null)
            {
                door.hinge.localRotation = Quaternion.identity;
            }
        }
    }
}