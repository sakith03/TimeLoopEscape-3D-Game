using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GhostSpawner — Student 4 (Agent Controller) — Helper Script
/// SE3032 Graphics & Visualization — TimeLoopEscape
///
/// Purpose:
///   - Spawns ghost agents into the scene at runtime
///   - In DEMO MODE: generates a random test path so you can see movement working
///     even before the IS module (A*/BFS) is integrated
///   - In LIVE MODE: receives path data from chathura (A*) or rivindu (BFS)
///     and forwards it to the correct ghost agent
///
/// HOW TO USE:
///   1. Create an empty GameObject in your scene, name it "GhostSpawner"
///   2. Attach this script to it
///   3. Assign ghostPrefab (your ghost_3d or ghost_baby prefab)
///   4. Assign graphManager
///   5. Enable demoMode to test movement before IS integration
/// </summary>
public class GhostSpawner : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Inspector Fields
    // ─────────────────────────────────────────────

    [Header("References")]
    [Tooltip("The ghost prefab (must have GhostAgentController attached)")]
    public GameObject ghostPrefab;

    [Tooltip("The GraphManager in the scene")]
    public GraphManager graphManager;

    [Header("Spawn Settings")]
    [Tooltip("How many ghosts to spawn")]
    public int ghostCount = 2;

    [Tooltip("Seconds to wait after Start before spawning (lets GraphManager build the graph first)")]
    public float spawnDelay = 1.5f;

    [Header("Demo Mode")]
    [Tooltip("Enable this to auto-generate random test paths. Disable when IS module provides real paths.")]
    public bool demoMode = true;

    [Tooltip("Length of random demo paths (number of waypoints)")]
    [Range(3, 20)]
    public int demoPathLength = 8;

    // ─────────────────────────────────────────────
    //  Private State
    // ─────────────────────────────────────────────

    // All spawned ghost agents, indexed so IS module can address them
    private List<GhostAgentController> spawnedAgents = new List<GhostAgentController>();

    // ─────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────

    void Start()
    {
        // Wait for GraphManager to finish building the graph before spawning
        Invoke(nameof(SpawnGhosts), spawnDelay);
    }

    // ─────────────────────────────────────────────
    //  Spawning
    // ─────────────────────────────────────────────

    void SpawnGhosts()
    {
        if (ghostPrefab == null)
        {
            Debug.LogError("[GhostSpawner] Ghost prefab is not assigned!");
            return;
        }

        if (graphManager == null)
        {
            Debug.LogError("[GhostSpawner] GraphManager is not assigned!");
            return;
        }

        if (graphManager.NodeCount == 0)
        {
            Debug.LogError("[GhostSpawner] GraphManager has 0 nodes — is NavMesh baked?");
            return;
        }

        for (int i = 0; i < ghostCount; i++)
        {
            // Pick a random spawn node
            int spawnNodeId = Random.Range(0, graphManager.NodeCount);
            Vector3 spawnPos = graphManager.GetNodePosition(spawnNodeId);

            // Instantiate the ghost
            GameObject ghostObj = Instantiate(ghostPrefab, spawnPos, Quaternion.identity);
            ghostObj.name = $"Ghost_{i}";

            // Get or add the controller
            GhostAgentController agent = ghostObj.GetComponent<GhostAgentController>();
            if (agent == null)
            {
                agent = ghostObj.AddComponent<GhostAgentController>();
                Debug.LogWarning($"[GhostSpawner] GhostAgentController was not on prefab — added automatically to Ghost_{i}.");
            }

            // Wire up the GraphManager reference
            agent.graphManager = graphManager;

            // Wire up the Animator if not already set
            if (agent.animator == null)
                agent.animator = ghostObj.GetComponentInChildren<Animator>();

            spawnedAgents.Add(agent);
            Debug.Log($"[GhostSpawner] Spawned Ghost_{i} at node {spawnNodeId} ({spawnPos})");

            // In demo mode, give it a random path immediately
            if (demoMode)
            {
                List<int> demoPath = GenerateRandomPath(spawnNodeId, demoPathLength);
                agent.SetPath(demoPath);
            }
        }
    }

    // ─────────────────────────────────────────────
    //  Public API  (called by IS module)
    // ─────────────────────────────────────────────

    /// <summary>
    /// Called by chathura (A*) or rivindu (BFS) to give a specific ghost a path.
    /// agentIndex: which ghost (0, 1, 2...)
    /// nodeIdPath: ordered list of GraphManager node IDs from start to goal
    /// </summary>
    public void AssignPath(int agentIndex, List<int> nodeIdPath)
    {
        if (agentIndex < 0 || agentIndex >= spawnedAgents.Count)
        {
            Debug.LogWarning($"[GhostSpawner] Agent index {agentIndex} out of range.");
            return;
        }

        spawnedAgents[agentIndex].SetPath(nodeIdPath);
    }

    /// <summary>
    /// Assign a path to ALL spawned ghosts (useful for group chase behaviour).
    /// </summary>
    public void AssignPathToAll(List<int> nodeIdPath)
    {
        foreach (var agent in spawnedAgents)
            agent.SetPath(new List<int>(nodeIdPath)); // each gets its own copy
    }

    /// <summary>
    /// Stop a specific ghost immediately.
    /// </summary>
    public void StopAgent(int agentIndex)
    {
        if (agentIndex >= 0 && agentIndex < spawnedAgents.Count)
            spawnedAgents[agentIndex].StopMoving();
    }

    /// <summary>
    /// Returns a spawned agent so IS module can reference it directly if needed.
    /// </summary>
    public GhostAgentController GetAgent(int agentIndex)
    {
        if (agentIndex >= 0 && agentIndex < spawnedAgents.Count)
            return spawnedAgents[agentIndex];
        return null;
    }

    /// <summary>
    /// Total number of spawned agents.
    /// </summary>
    public int AgentCount => spawnedAgents.Count;

    // ─────────────────────────────────────────────
    //  Demo Path Generation
    // ─────────────────────────────────────────────

    /// <summary>
    /// Generates a random walk through the graph starting from startNodeId.
    /// Used only in demoMode — replace with real A*/BFS output when IS is ready.
    /// </summary>
    List<int> GenerateRandomPath(int startNodeId, int length)
    {
        List<int> path = new List<int> { startNodeId };
        int current = startNodeId;

        for (int i = 0; i < length - 1; i++)
        {
            var neighbors = graphManager.GetNeighbors(current);
            if (neighbors == null || neighbors.Count == 0) break;

            // Pick a random neighbor
            int nextIdx = Random.Range(0, neighbors.Count);
            int nextNode = neighbors[nextIdx].neighbor;

            // Avoid going back to the previous node (makes paths less jittery)
            if (path.Count > 1 && nextNode == path[path.Count - 2] && neighbors.Count > 1)
            {
                nextIdx = (nextIdx + 1) % neighbors.Count;
                nextNode = neighbors[nextIdx].neighbor;
            }

            path.Add(nextNode);
            current = nextNode;
        }

        return path;
    }
}
