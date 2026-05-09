using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    public AStarPathfinder pathfinder;

    // Add all destination targets to this list
    public List<Transform> targets;

    public float speed = 3f;

    private List<Vector3> path;

    private int nodeIndex = 0;

    private int targetIndex = 0;

    private bool isMoving = false;

    void Start()
    {
        // Start navigation if targets are assigned
        if (targets != null && targets.Count > 0)
        {
            StartCoroutine(StartNavigation());
        }
    }

    IEnumerator StartNavigation()
    {
        // Wait until GraphManager finishes generating the graph
        yield return new WaitForSeconds(0.5f);

        MoveToNextTarget();
    }

    void MoveToNextTarget()
    {
        // Stop if all targets are completed
        if (targetIndex >= targets.Count)
        {
            Debug.Log("Finished visiting all targets!");
            isMoving = false;
            return;
        }

        // Find shortest path from current position to next target
        path = pathfinder.FindShortestPath(
            transform.position,
            targets[targetIndex].position
        );

        // If a valid path exists
        if (path != null && path.Count > 0)
        {
            nodeIndex = 0;
            isMoving = true;
        }
        else
        {
            // If no path found, skip to next target
            Debug.LogError("Cannot find path to target " + targetIndex);

            targetIndex++;

            MoveToNextTarget();
        }
    }

    void Update()
    {
        if (!isMoving || path == null)
            return;

        // Move through all nodes in the current path
        if (nodeIndex < path.Count)
        {
            Vector3 targetNodePos = path[nodeIndex];

            // Move NPC toward current node
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetNodePos,
                speed * Time.deltaTime
            );

            // Rotate NPC toward movement direction
            Vector3 direction =
                (targetNodePos - transform.position).normalized;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation =
                    Quaternion.LookRotation(
                        new Vector3(direction.x, 0, direction.z)
                    );

                transform.rotation =
                    Quaternion.Slerp(
                        transform.rotation,
                        lookRotation,
                        Time.deltaTime * 5f
                    );
            }

            // Move to next node when close enough
            if (Vector3.Distance(transform.position, targetNodePos) < 0.2f)
            {
                nodeIndex++;
            }
        }
        else
        {
            // Current target reached, move to next target
            isMoving = false;

            targetIndex++;

            MoveToNextTarget();
        }
    }
}