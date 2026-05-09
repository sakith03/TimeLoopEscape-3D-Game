using System.Collections.Generic;
using UnityEngine;



public class EnvironmentEventInterceptor : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Inspector References
    // -----------------------------------------------------------------------

    [Header("Core References")]
    [Tooltip("The GraphManager in the scene")]
    public GraphManager graphManager;

    [Tooltip("All GhostAgentControllers in the scene. Add them all here.")]
    public GhostAgentController[] agents;

    [Header("Recalculation Settings")]
    [Tooltip("Minimum seconds between recalculations. Prevents infinite loops if " +
             "multiple events fire at the same frame.")]
    public float recalculationCooldown = 0.5f;

    [Tooltip("If true, prints a detailed log every time an edge is severed or restored")]
    public bool verboseLogging = true;

    // -----------------------------------------------------------------------
    // Edge Restoration Cache
    // -----------------------------------------------------------------------
    // GraphManager.RemoveEdge() destroys the weight. We cache it here so we
    // can rebuild the edge with the correct distance when an object is removed.
    //
    // Key:   "nodeA_nodeB" (always lower ID first for consistency)
    // Value: EdgeRecord containing weight and source object
    // -----------------------------------------------------------------------

    [System.Serializable]
    public class EdgeRecord
    {
        public int nodeA;
        public int nodeB;
        public float weight;          // Euclidean distance, same as GraphManager built it
        public GameObject source;     // Which barricade/door caused this severance
        public float severedAt;       // Time.time when severed
    }

    // Active severances: edges currently removed from the graph
    private Dictionary<string, EdgeRecord> severedEdges
        = new Dictionary<string, EdgeRecord>();

    // Cooldown guard
    private float lastRecalculationTime = -999f;

    // Event subscribers (A* and BFS modules can hook in here)
    public System.Action<int, int> OnEdgeSeveredEvent;   // fired after severance
    public System.Action<int, int> OnEdgeRestoredEvent;  // fired after restoration
    public System.Action OnRecalculationTriggered;        // fired when agents need new paths

    // -----------------------------------------------------------------------
    // Unity Lifecycle
    // -----------------------------------------------------------------------

    void Start()
    {
        // Auto-find agents if not manually assigned
        if (agents == null || agents.Length == 0)
        {
            agents = FindObjectsByType<GhostAgentController>(FindObjectsSortMode.None);
            Debug.Log($"[EEI] Auto-found {agents.Length} GhostAgentController(s) in scene.");
        }

        if (graphManager == null)
        {
            Debug.LogError("[EEI] GraphManager not assigned! Assign it in the Inspector.");
        }

        Debug.Log("[EnvironmentEventInterceptor] Initialized. Watching for environment changes.");
    }

    // -----------------------------------------------------------------------
    // PUBLIC API — Called by BarricadeInteractable and DoorInteractable
    // -----------------------------------------------------------------------

    /// <summary>
    /// Call this when a barricade or door BLOCKS a path.
    /// Caches the edge weight for later restoration, then triggers recalculation.
    /// </summary>
    public void OnEdgeSevered(int nodeA, int nodeB, GameObject source)
    {
        string key = EdgeKey(nodeA, nodeB);

        // Prevent double-registration of the same edge
        if (severedEdges.ContainsKey(key))
        {
            if (verboseLogging)
                Debug.Log($"[EEI] Edge {key} already severed by {severedEdges[key].source.name}. Skipping.");
            return;
        }

        // Calculate and cache the edge weight (Euclidean distance between nodes)
        // GraphManager already removed it, so we compute from world positions
        Vector3 posA = graphManager.GetNodePosition(nodeA);
        Vector3 posB = graphManager.GetNodePosition(nodeB);
        float weight = Vector3.Distance(posA, posB);

        EdgeRecord record = new EdgeRecord
        {
            nodeA      = nodeA,
            nodeB      = nodeB,
            weight     = weight,
            source     = source,
            severedAt  = Time.time
        };

        severedEdges[key] = record;

        if (verboseLogging)
            Debug.Log($"[EEI] Edge SEVERED — node {nodeA} <-> node {nodeB} " +
                      $"(weight: {weight:F2}m) by [{source.name}]. " +
                      $"Total blocked edges: {severedEdges.Count}");

        // Notify A* and BFS modules
        OnEdgeSeveredEvent?.Invoke(nodeA, nodeB);

        // Trigger agent recalculation (with cooldown guard)
        TriggerRecalculation();
    }

    /// <summary>
    /// Call this when a barricade is picked up or a door is opened.
    /// Rebuilds the edge in the graph and recalculates agent paths.
    /// </summary>
    public void OnEdgeRestored(int nodeA, int nodeB, GameObject source)
    {
        string key = EdgeKey(nodeA, nodeB);

        if (!severedEdges.ContainsKey(key))
        {
            if (verboseLogging)
                Debug.LogWarning($"[EEI] Tried to restore edge {key} but it was never recorded.");
            return;
        }

        EdgeRecord record = severedEdges[key];

        // Rebuild the edge in both directions in the adjacency list
        // GraphManager.GetAdjacency() returns the live dictionary — we write directly
        var adjacency = graphManager.GetAdjacency();

        if (adjacency.ContainsKey(nodeA))
            adjacency[nodeA].Add((nodeB, record.weight));

        if (adjacency.ContainsKey(nodeB))
            adjacency[nodeB].Add((nodeA, record.weight));

        severedEdges.Remove(key);

        if (verboseLogging)
            Debug.Log($"[EEI] Edge RESTORED — node {nodeA} <-> node {nodeB} " +
                      $"(weight: {record.weight:F2}m). " +
                      $"Remaining blocked edges: {severedEdges.Count}");

        // Notify A* and BFS modules
        OnEdgeRestoredEvent?.Invoke(nodeA, nodeB);

        // Recalculate so agents can now use the restored path
        TriggerRecalculation();
    }

    // -----------------------------------------------------------------------
    // RECALCULATION ENGINE
    // This is the key IS2 mechanism — after graph changes, all active agents
    // must be told to request a new path from A* or BFS.
    // -----------------------------------------------------------------------

    /// <summary>
    /// Triggers path recalculation on all active agents.
    /// Protected by a cooldown to prevent infinite-loop recalculations if
    /// multiple events fire in the same frame (e.g., a thrown barricade that
    /// touches multiple triggers simultaneously).
    /// </summary>
    public void TriggerRecalculation()
    {
        // === INFINITE LOOP GUARD ===
        // If recalculation was triggered less than cooldownSeconds ago, skip.
        // This is the anti-infinite-loop mechanism required by the IS rubric.
        if (Time.time - lastRecalculationTime < recalculationCooldown)
        {
            if (verboseLogging)
                Debug.Log($"[EEI] Recalculation skipped — cooldown active " +
                          $"({recalculationCooldown - (Time.time - lastRecalculationTime):F2}s remaining).");
            return;
        }

        lastRecalculationTime = Time.time;

        if (verboseLogging)
            Debug.Log($"[EEI] >>> RECALCULATION TRIGGERED at t={Time.time:F2}s " +
                      $"— notifying {agents.Length} agent(s).");

        // Notify all subscriber modules (A*, BFS)
        OnRecalculationTriggered?.Invoke();

        // Tell each agent to request a new path
        // GhostAgentController.RequestNewPath() is the hook into the GV module
        foreach (var agent in agents)
        {
            if (agent == null) continue;

            if (agent.IsMoving)
            {
                if (verboseLogging)
                    Debug.Log($"[EEI] Requesting new path for agent: {agent.name}");

                agent.RequestNewPath();
            }
        }
    }

    // -----------------------------------------------------------------------
    // UTILITY
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns a consistent dictionary key for an edge regardless of direction.
    /// Always uses the lower node ID first: "5_12" not "12_5"
    /// </summary>
    private string EdgeKey(int nodeA, int nodeB)
    {
        return nodeA < nodeB ? $"{nodeA}_{nodeB}" : $"{nodeB}_{nodeA}";
    }

    /// <summary>
    /// Returns all currently severed edges. Useful for A* and BFS to check
    /// before pathfinding if certain nodes are currently unreachable.
    /// </summary>
    public List<EdgeRecord> GetAllSeveredEdges()
    {
        return new List<EdgeRecord>(severedEdges.Values);
    }

    /// <summary>
    /// Returns true if the edge between nodeA and nodeB is currently severed.
    /// A* and BFS can call this to pre-validate paths.
    /// </summary>
    public bool IsEdgeSevered(int nodeA, int nodeB)
    {
        return severedEdges.ContainsKey(EdgeKey(nodeA, nodeB));
    }

    // -----------------------------------------------------------------------
    // DEBUG — Draw severed edges in Scene view as red lines
    // -----------------------------------------------------------------------
    void OnDrawGizmos()
    {
        if (graphManager == null || severedEdges == null) return;

        Gizmos.color = Color.red;
        foreach (var record in severedEdges.Values)
        {
            // Safety check: node IDs may be out of range if graph hasn't built yet
            if (record.nodeA >= graphManager.NodeCount ||
                record.nodeB >= graphManager.NodeCount) continue;

            Vector3 a = graphManager.GetNodePosition(record.nodeA);
            Vector3 b = graphManager.GetNodePosition(record.nodeB);

            // Draw a thick red X over severed edges
            Gizmos.DrawLine(a, b);
            Gizmos.DrawSphere((a + b) / 2f, 0.2f); // midpoint marker
        }
    }
}
