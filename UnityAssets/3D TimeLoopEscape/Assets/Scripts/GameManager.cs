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

        // Find all doors
        doors = FindObjectsOfType<DoorController>();

        // Initial timer display
        if (timerText != null)
        {
            timerText.text = "Time Left: " + Mathf.Ceil(timer).ToString();
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;

        // Update timer UI
        if (timerText != null)
        {
            timerText.text = "Time Left: " + Mathf.Ceil(timer).ToString();
        }

        if (timer <= 0f)
        {
            ResetLoop();
        }
    }

    void ResetLoop()
    {
        timer = loopDuration;

        Debug.Log("🔁 Full Loop Reset");

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

        // If you have custom Graph logic,
        // call its reset function here if needed.
        // Example:
        // npcGhost.GetComponent<Pathfinding_Logic>()?.ResetPath();
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