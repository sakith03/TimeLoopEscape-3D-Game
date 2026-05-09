using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GhostAgentController — Student 4 (Agent Controller)
/// SE3032 Graphics & Visualization — TimeLoopEscape
///
/// Responsibilities:
///   - Receives a path as a List<int> of GraphManager node IDs
///   - Converts node IDs to world-space Vector3 waypoints
///   - Moves the ghost smoothly along the path with acceleration/deceleration
///   - Rotates the ghost naturally to face the direction of travel
///   - Drives the GhostAnimator controller (Idle / Walk / Run states)
///
/// HOW TO USE:
///   1. Attach this script to your Ghost GameObject (ghost_3d or ghost_baby prefab)
///   2. Assign the GraphManager reference in the Inspector
///   3. Assign the Animator reference in the Inspector
///   4. When the IS module (chathura/rivindu) produces a path, call:
///         ghostAgent.SetPath(listOfNodeIds);
/// </summary>
public class GhostAgentController : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Inspector Fields
    // ─────────────────────────────────────────────

    [Header("References")]
    [Tooltip("Drag the GameObject that has GraphManager attached")]
    public GraphManager graphManager;

    [Tooltip("Drag the Animator component of this Ghost")]
    public Animator animator;

    [Header("Movement")]
    [Tooltip("Maximum movement speed (units/sec)")]
    public float moveSpeed = 3.5f;

    [Tooltip("How quickly the agent accelerates to full speed")]
    public float acceleration = 4f;

    [Tooltip("How quickly the agent decelerates near a waypoint")]
    public float deceleration = 6f;

    [Tooltip("Distance to waypoint before moving to the next one")]
    public float waypointReachedThreshold = 0.25f;

    [Header("Rotation")]
    [Tooltip("Degrees per second for smooth turning")]
    public float rotationSpeed = 180f;

    [Header("Animation Thresholds")]
    [Tooltip("Speed below this = Idle animation")]
    public float idleThreshold = 0.05f;

    [Tooltip("Speed above this = Run animation (between idle and run = Walk)")]
    public float runThreshold = 2.8f;

    [Header("Debug")]
    [Tooltip("Draw the path as coloured lines in the Scene view")]
    public bool drawDebugPath = true;

    // ─────────────────────────────────────────────
    //  IS2 Integration Fields
    //  SE3062 - Student 2 - Dynamic Adaptation
    // ─────────────────────────────────────────────

    [Header("IS Module Integration")]
    [Tooltip("Reference to the IS2 EnvironmentEventInterceptor. Assign in Inspector.")]
    public EnvironmentEventInterceptor environmentInterceptor;

    /// <summary>
    /// The destination node ID for this agent.
    /// Set by A* or BFS when assigning a path.
    /// EnvironmentEventInterceptor reads this to know where to re-route to.
    /// </summary>
    [HideInInspector] public int destinationNodeId = -1;

    /// <summary>
    /// Returns the graph node ID closest to this agent's current world position.
    /// IS module uses this as the start node when recalculating after a graph change.
    /// </summary>
    public int CurrentNodeId
    {
        get
        {
            if (graphManager == null) return -1;
            return graphManager.NearestNode(transform.position);
        }
    }

    // ─────────────────────────────────────────────
    //  Private State
    // ─────────────────────────────────────────────

    // The current world-space waypoints the agent is following
    private List<Vector3> waypoints = new List<Vector3>();

    // Index into waypoints[] for the NEXT target
    private int currentWaypointIndex = 0;

    // Current speed (smoothly ramped between 0 and moveSpeed)
    private float currentSpeed = 0f;

    // Is the agent actively moving along a path?
    private bool isMoving = false;

    // Cached animator parameter hashes for performance
    private static readonly int HashSpeed     = Animator.StringToHash("Speed");
    private static readonly int HashIsWalking = Animator.StringToHash("isWalking");
    private static readonly int HashIsRunning = Animator.StringToHash("isRunning");

    // ─────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────

    void Start()
    {
        if (graphManager == null)
            Debug.LogError($"[GhostAgentController] '{gameObject.name}': GraphManager is not assigned!");

        if (animator == null)
            Debug.LogError($"[GhostAgentController] '{gameObject.name}': Animator is not assigned!");

        SetAnimationState(0f);
    }

    void Update()
    {
        if (!isMoving || waypoints.Count == 0) return;

        MoveAlongPath();
    }

    // ─────────────────────────────────────────────
    //  Public API  (called by IS module)
    // ─────────────────────────────────────────────

    /// <summary>
    /// Called by the IS module (A* / BFS) to give this ghost a new path.
    /// Pass a list of GraphManager node IDs in order from start to goal.
    /// </summary>
    public void SetPath(List<int> nodeIdPath)
    {
        if (nodeIdPath == null || nodeIdPath.Count == 0)
        {
            Debug.LogWarning($"[GhostAgentController] '{gameObject.name}': Received an empty path.");
            StopMoving();
            return;
        }

        // Store the destination node for IS2 recalculation reference
        destinationNodeId = nodeIdPath[nodeIdPath.Count - 1];

        // Convert node IDs → world positions
        waypoints.Clear();
        foreach (int id in nodeIdPath)
        {
            waypoints.Add(graphManager.GetNodePosition(id));
        }

        currentWaypointIndex = 0;
        currentSpeed = 0f;
        isMoving = true;

        Debug.Log($"[GhostAgentController] '{gameObject.name}': Path set with {waypoints.Count} waypoints. Destination node: {destinationNodeId}");
    }

    /// <summary>
    /// Immediately stops the agent and plays the Idle animation.
    /// </summary>
    public void StopMoving()
    {
        isMoving = false;
        currentSpeed = 0f;
        waypoints.Clear();
        SetAnimationState(0f);
    }

    /// <summary>
    /// Returns true if the agent is currently following a path.
    /// </summary>
    public bool IsMoving => isMoving;

    /// <summary>
    /// Called by EnvironmentEventInterceptor (IS2) when the graph changes.
    /// Stops the agent immediately, then triggers recalculation via A* or BFS.
    /// </summary>
    public void RequestNewPath()
    {
        if (destinationNodeId == -1)
        {
            // No destination assigned yet — nothing to recalculate
            return;
        }

        int startNode = CurrentNodeId;

        if (startNode == -1)
        {
            Debug.LogWarning($"[GhostAgent:{name}] RequestNewPath() called but " +
                             "cannot determine current node position.");
            return;
        }

        Debug.Log($"[GhostAgent:{name}] Path recalculation requested. " +
                  $"Start: node {startNode}, Destination: node {destinationNodeId}");

        // Stop immediately — don't walk into a newly blocked path
        StopMoving();

        // =========================================================
        // INTEGRATION POINT FOR A* (Student 3 - Chathura):
        //
        //   List<int> newPath = aStarPathfinder.FindPath(startNode, destinationNodeId);
        //   if (newPath != null && newPath.Count > 0)
        //       SetPath(newPath);
        //
        // INTEGRATION POINT FOR BFS (Student 4 - Rivindu):
        //
        //   List<int> newPath = bfsPathfinder.FindPath(startNode, destinationNodeId);
        //   if (newPath != null && newPath.Count > 0)
        //       SetPath(newPath);
        // =========================================================

        Debug.Log($"[GhostAgent:{name}] Awaiting pathfinder. Call SetPath(List<int>) to resume.");
    }

    // ─────────────────────────────────────────────
    //  Core Movement Logic
    // ─────────────────────────────────────────────

    void MoveAlongPath()
    {
        Vector3 target   = waypoints[currentWaypointIndex];
        Vector3 toTarget = target - transform.position;
        toTarget.y = 0f; // keep movement flat on the ground plane

        float distanceToWaypoint = toTarget.magnitude;

        // ── Reached current waypoint? ──
        if (distanceToWaypoint < waypointReachedThreshold)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Count)
            {
                StopMoving();
                Debug.Log($"[GhostAgentController] '{gameObject.name}': Path complete.");
                return;
            }

            target   = waypoints[currentWaypointIndex];
            toTarget = target - transform.position;
            toTarget.y = 0f;
            distanceToWaypoint = toTarget.magnitude;
        }

        // ── Smooth rotation ──
        if (toTarget.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(toTarget.normalized);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // ── Acceleration / Deceleration ──
        float decelerationDistance = (currentSpeed * currentSpeed) / (2f * deceleration);
        bool shouldDecelerate = distanceToWaypoint < decelerationDistance + waypointReachedThreshold;

        if (shouldDecelerate)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, moveSpeed, acceleration * Time.deltaTime);
        }

        // ── Apply movement ──
        if (currentSpeed > 0.001f)
        {
            transform.position += toTarget.normalized * currentSpeed * Time.deltaTime;
        }

        // ── Drive animator ──
        SetAnimationState(currentSpeed);
    }

    // ─────────────────────────────────────────────
    //  Animation
    // ─────────────────────────────────────────────

    void SetAnimationState(float speed)
    {
        if (animator == null) return;

        // Set the Speed float — drives all transitions in NPC_Animator
        SafeSetFloat(HashSpeed, speed);

        // Bool fallbacks in case the animator uses them instead of Speed
        SafeSetBool(HashIsWalking, speed > idleThreshold && speed <= runThreshold);
        SafeSetBool(HashIsRunning, speed > runThreshold);
    }

    // Safe setters — won't throw if the parameter doesn't exist in the animator
    void SafeSetFloat(int hash, float value)
    {
        foreach (var param in animator.parameters)
            if (param.nameHash == hash && param.type == AnimatorControllerParameterType.Float)
            {
                animator.SetFloat(hash, value);
                return;
            }
    }

    void SafeSetBool(int hash, bool value)
    {
        foreach (var param in animator.parameters)
            if (param.nameHash == hash && param.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(hash, value);
                return;
            }
    }

    // ─────────────────────────────────────────────
    //  Debug Gizmos
    // ─────────────────────────────────────────────

    void OnDrawGizmos()
    {
        if (!drawDebugPath || waypoints == null || waypoints.Count == 0) return;

        // Draw path segments
        Gizmos.color = Color.green;
        for (int i = 0; i < waypoints.Count - 1; i++)
            Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);

        // Draw waypoint spheres — yellow = visited, green = upcoming
        for (int i = 0; i < waypoints.Count; i++)
        {
            Gizmos.color = (i < currentWaypointIndex) ? Color.yellow : Color.green;
            Gizmos.DrawSphere(waypoints[i], 0.15f);
        }

        // Draw current target in red
        if (currentWaypointIndex < waypoints.Count)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(waypoints[currentWaypointIndex], 0.2f);
        }
    }
}
