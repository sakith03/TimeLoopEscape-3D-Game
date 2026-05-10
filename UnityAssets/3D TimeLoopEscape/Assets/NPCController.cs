using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    public AStarPathfinder pathfinder;

    public List<Transform> targets;

    public float speed = 3f;

    private List<Vector3> path;
    private int nodeIndex = 0;
    private int targetIndex = 0;
    private bool isMoving = false;

    void Start()
    {
        if (targets != null && targets.Count > 0)
        {
            StartCoroutine(StartNavigation());
        }
    }

    IEnumerator StartNavigation()
    {
        yield return new WaitForSeconds(0.5f);
        MoveToNextTarget();
    }

    void MoveToNextTarget()
    {
        if (targetIndex >= targets.Count)
        {
            Debug.Log("Finished visiting all targets!");
            isMoving = false;
            return;
        }

        if (SearchModeManager.Instance.currentMode == SearchModeManager.SearchMode.BFS)
        {
            BFS_Search bfs = new BFS_Search(FindFirstObjectByType<GraphManager>());

            var result = bfs.FindPath(
                transform.position,
                targets[targetIndex].position
            );

            path = result.path;

            if (DebugVisualizer.Instance != null)
            {
                DebugVisualizer.Instance.SetData(
                    result.explored,
                    result.frontier,
                    path
                );
            }
        }
        else
        {
            path = pathfinder.FindShortestPath(
                transform.position,
                targets[targetIndex].position
            );
        }

        if (path != null && path.Count > 0)
        {
            nodeIndex = 0;
            isMoving = true;
        }
        else
        {
            Debug.LogError("Cannot find path to target " + targetIndex);
            targetIndex++;
            MoveToNextTarget();
        }
    }

    void Update()
    {
        if (!isMoving || path == null)
            return;

        if (nodeIndex < path.Count)
        {
            Vector3 targetNodePos = path[nodeIndex];

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetNodePos,
                speed * Time.deltaTime
            );

            Vector3 direction = (targetNodePos - transform.position).normalized;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation =
                    Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

                transform.rotation =
                    Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }

            if (Vector3.Distance(transform.position, targetNodePos) < 0.2f)
            {
                nodeIndex++;
            }
        }
        else
        {
            isMoving = false;
            targetIndex++;
            MoveToNextTarget();
        }
    }
}