using System.Collections.Generic;
using UnityEngine;

public class BFS_Search
{
    private GraphManager graph;

    public BFS_Search(GraphManager graphManager)
    {
        graph = graphManager;
    }

    public (List<Vector3> path, List<int> explored, List<int> frontier) FindPath(Vector3 startPos, Vector3 goalPos)
    {
        List<int> explored = new List<int>();
        List<int> frontier = new List<int>();

        int startNode = graph.NearestNode(startPos);
        int goalNode = graph.NearestNode(goalPos);

        if (startNode == -1 || goalNode == -1)
            return (new List<Vector3>(), explored, frontier);

        if (startNode == goalNode)
        {
            return (new List<Vector3> { graph.GetNodePosition(startNode) }, explored, frontier);
        }

        Queue<int> queue = new Queue<int>();
        Dictionary<int, int> cameFrom = new Dictionary<int, int>();
        HashSet<int> visited = new HashSet<int>();

        queue.Enqueue(startNode);
        visited.Add(startNode);
        frontier.Add(startNode);

        bool found = false;

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            explored.Add(current);

            if (current == goalNode)
            {
                found = true;
                break;
            }

            foreach (var (neighbor, weight) in graph.GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                    frontier.Add(neighbor);
                }
            }
        }

        if (!found)
            return (new List<Vector3>(), explored, frontier);

        List<Vector3> path = ReconstructPath(cameFrom, startNode, goalNode);
        return (path, explored, frontier);
    }

    private List<Vector3> ReconstructPath(Dictionary<int, int> cameFrom, int startNode, int goalNode)
    {
        List<Vector3> path = new List<Vector3>();

        int current = goalNode;
        path.Add(graph.GetNodePosition(current));

        while (current != startNode)
        {
            current = cameFrom[current];
            path.Add(graph.GetNodePosition(current));
        }

        path.Reverse();
        return path;
    }
}