using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GraphManager : MonoBehaviour
{
    [Header("Sampling")]
    public float gridSpacing = 2f;      // distance between sample points
    public float sampleRadius = 1f;     // how close a point must be to NavMesh to count
    public Vector3 gridCenter = Vector3.zero;
    public int gridSize = 20;           // samples a (gridSize x gridSize) grid

    [Header("Edge Building")]
    public float maxEdgeDistance = 3f;  // max distance between two nodes to consider an edge
    public LayerMask obstacleLayer;     // set this to your walls/obstacles layer in Inspector

    // --- The graph data structures ---
    // Each node: its ID and world position
    private List<Vector3> nodes = new List<Vector3>();

    // Adjacency list: nodeId -> list of (neighborId, edgeWeight)
    private Dictionary<int, List<(int neighbor, float weight)>> adjacency
        = new Dictionary<int, List<(int, float)>>();

    // Called once when the game starts
    void Start()
    {
        SampleNavMesh();
        BuildEdges();
        PrintGraph();
    }

    // Sample the NavMesh into discrete nodes
    void SampleNavMesh()
    {
        nodes.Clear();
        adjacency.Clear();

        // Walk a grid across the scene
        for (int x = -gridSize; x <= gridSize; x++)
        {
            for (int z = -gridSize; z <= gridSize; z++)
            {
                // Candidate position in world space
                Vector3 candidate = gridCenter + new Vector3(
                    x * gridSpacing, 0, z * gridSpacing);

                NavMeshHit hit;
                // Ask Unity: is there a valid NavMesh point near this candidate?
                if (NavMesh.SamplePosition(candidate, out hit, sampleRadius, NavMesh.AllAreas))
                {
                    // Store the snapped NavMesh position as a node
                    int id = nodes.Count;
                    nodes.Add(hit.position);
                    adjacency[id] = new List<(int, float)>();
                }
            }
        }

        Debug.Log($"[GraphManager] Sampled {nodes.Count} nodes from NavMesh.");
    }
    void BuildEdges()
    {
        int edgeCount = 0;

        for (int a = 0; a < nodes.Count; a++)
        {
            for (int b = a + 1; b < nodes.Count; b++)
            {
                float dist = Vector3.Distance(nodes[a], nodes[b]);

                // Only check nodes within max distance (performance cutoff)
                if (dist > maxEdgeDistance) continue;

                Vector3 direction = (nodes[b] - nodes[a]).normalized;
                float rayHeight = 0.5f; // cast slightly above ground

                Vector3 rayStart = nodes[a] + Vector3.up * rayHeight;

                // Cast a ray between the two nodes
                // If nothing blocks it, they can see each other -> add edge
                if (!Physics.Raycast(rayStart, direction, dist, obstacleLayer))
                {
                    // Bidirectional edge: a->b and b->a
                    adjacency[a].Add((b, dist));
                    adjacency[b].Add((a, dist));
                    edgeCount++;
                }
            }
        }

        Debug.Log($"[GraphManager] Built {edgeCount} edges.");
    }

    // Public APIs
    // chathura(A*) and rivindu(BFS) call this every step
    public List<(int neighbor, float weight)> GetNeighbors(int nodeId)
    {
        if (adjacency.ContainsKey(nodeId))
            return adjacency[nodeId];
        return new List<(int, float)>();
    }

    // chathura call this when a barricade is placed
    public void RemoveEdge(int nodeA, int nodeB)
    {
        if (adjacency.ContainsKey(nodeA))
            adjacency[nodeA].RemoveAll(e => e.neighbor == nodeB);

        if (adjacency.ContainsKey(nodeB))
            adjacency[nodeB].RemoveAll(e => e.neighbor == nodeA);

        Debug.Log($"[GraphManager] Edge removed between node {nodeA} and {nodeB}.");
    }
public int NearestNode(Vector3 worldPos)
{
    if (nodes == null || nodes.Count == 0)
    {
        Debug.LogError("[GraphManager] Nodes list eka empty! SampleNavMesh() hariyata run unada balanna.");
        return -1;
    }

    int closest = -1;
    float bestDist = float.MaxValue;

    for (int i = 0; i < nodes.Count; i++)
    {
        float d = Vector3.Distance(worldPos, nodes[i]);
        if (d < bestDist)
        {
            bestDist = d;
            closest = i;
        }
    }
    return closest;
}
    // Returns the world position of a node by its ID
    public Vector3 GetNodePosition(int nodeId)
    {
        return nodes[nodeId];
    }

    // Returns total node count (useful for A* and BFS init)
    public int NodeCount => nodes.Count;

    public List<Vector3> GetAllNodes()
    {
        return nodes;
    }

    public Dictionary<int, List<(int neighbor, float weight)>> GetAdjacency()
    {
        return adjacency;
    }

    // testing Print adjacency list to console
    void PrintGraph()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            string line = $"Node {i} at {nodes[i]:F1} -> ";
            foreach (var (neighbor, weight) in adjacency[i])
            {
                line += $"[{neighbor}, dist:{weight:F2}] ";
            }
            Debug.Log(line);
        }
    }
    // draw nodes and edges in scene for visual testing
    void OnDrawGizmos()
    {
        if (nodes == null) return;

        // draw each node as a yellow sphere
        Gizmos.color = Color.yellow;
        foreach (var node in nodes)
            Gizmos.DrawSphere(node, 0.15f);

        // draw each edge as a cyan line
        Gizmos.color = Color.cyan;
        for (int a = 0; a < nodes.Count; a++)
            foreach (var (b, w) in adjacency[a])
                if (b > a)
                    Gizmos.DrawLine(nodes[a], nodes[b]);
    }
}

