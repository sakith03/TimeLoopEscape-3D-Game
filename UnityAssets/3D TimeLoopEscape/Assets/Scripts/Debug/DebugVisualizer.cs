using System.Collections.Generic;
using UnityEngine;

public class DebugVisualizer : MonoBehaviour
{
    public static DebugVisualizer Instance;

    public KeyCode toggleKey = KeyCode.F3;
    public bool debugMode = false;

    private List<int> exploredNodes = new List<int>();
    private List<int> frontierNodes = new List<int>();
    private List<Vector3> finalPath = new List<Vector3>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            debugMode = !debugMode;
            Debug.Log("Debug Mode: " + debugMode);
        }
    }

    public void SetData(List<int> explored, List<int> frontier, List<Vector3> path)
    {
        exploredNodes = explored;
        frontierNodes = frontier;
        finalPath = path;
    }

    void OnDrawGizmos()
    {
        if (!debugMode) return;

        GraphManager graph = FindFirstObjectByType<GraphManager>();
        if (graph == null) return;

        Gizmos.color = Color.blue;
        foreach (int nodeId in frontierNodes)
            Gizmos.DrawSphere(graph.GetNodePosition(nodeId), 0.18f);

        Gizmos.color = Color.yellow;
        foreach (int nodeId in exploredNodes)
            Gizmos.DrawSphere(graph.GetNodePosition(nodeId), 0.25f);

        if (finalPath != null && finalPath.Count > 1)
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < finalPath.Count - 1; i++)
                Gizmos.DrawLine(finalPath[i], finalPath[i + 1]);
        }
    }
}