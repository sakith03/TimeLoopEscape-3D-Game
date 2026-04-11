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
}