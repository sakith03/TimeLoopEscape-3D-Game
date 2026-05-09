using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder : MonoBehaviour
{
    public GraphManager graphManager;

    // Mathematically justified Euclidean Heuristic
    float Heuristic(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b);
    }

    public List<Vector3> FindShortestPath(Vector3 startPos, Vector3 targetPos)
    {
        int startNode = graphManager.NearestNode(startPos);
        int targetNode = graphManager.NearestNode(targetPos);

        Debug.Log($"[AStar] Start Pos: {startPos}, Nearest Node ID: {startNode}");
        Debug.Log($"[AStar] Target Pos: {targetPos}, Nearest Node ID: {targetNode}");

        if (startNode == -1 || targetNode == -1) return null;

        // Priority Queue implementation using a SortedList for efficiency
        var openSet = new List<int> { startNode };
        var gScore = new Dictionary<int, float>();
        var fScore = new Dictionary<int, float>();
        var parentMap = new Dictionary<int, int>();

        for (int i = 0; i < graphManager.NodeCount; i++)
        {
            gScore[i] = float.MaxValue;
            fScore[i] = float.MaxValue;
        }

        gScore[startNode] = 0;
        fScore[startNode] = Heuristic(graphManager.GetNodePosition(startNode), graphManager.GetNodePosition(targetNode));

        while (openSet.Count > 0)
        {
            // Pick node with lowest fScore (Priority Queue logic)
            int current = openSet[0];
            foreach (int node in openSet)
            {
                if (fScore[node] < fScore[current]) current = node;
            }

            if (current == targetNode) return ReconstructPath(parentMap, current);

            openSet.Remove(current);

            foreach (var edge in graphManager.GetNeighbors(current))
            {
                float tentativeGScore = gScore[current] + edge.weight;

                if (tentativeGScore < gScore[edge.neighbor])
                {
                    parentMap[edge.neighbor] = current;
                    gScore[edge.neighbor] = tentativeGScore;
                    fScore[edge.neighbor] = gScore[edge.neighbor] + Heuristic(graphManager.GetNodePosition(edge.neighbor), graphManager.GetNodePosition(targetNode));

                    if (!openSet.Contains(edge.neighbor))
                        openSet.Add(edge.neighbor);
                }
            }
        }
        return null;
    }

    List<Vector3> ReconstructPath(Dictionary<int, int> parentMap, int current)
    {
        List<Vector3> path = new List<Vector3>();
        path.Add(graphManager.GetNodePosition(current));
        while (parentMap.ContainsKey(current))
        {
            current = parentMap[current];
            path.Add(graphManager.GetNodePosition(current));
        }
        path.Reverse();
        return path;
    }
}