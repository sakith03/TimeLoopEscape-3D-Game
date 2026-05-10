using UnityEngine;



public class DoorInteractable : MonoBehaviour
{
    [Header("Graph References")]
    public GraphManager graphManager;
    public EnvironmentEventInterceptor interceptor;

    [Header("Door State")]
    [Tooltip("Start closed? If true, edge is severed on Start()")]
    public bool startClosed = false;

    [Header("Node Override (Optional)")]
    [Tooltip("If you know the exact node IDs on each side of the door, set them here. " +
             "Leave -1 to auto-detect using NearestNode().")]
    public int manualNodeA = -1;
    public int manualNodeB = -1;

    [Header("Detection")]
    [Tooltip("Sample this far in front of and behind the door to find graph nodes")]
    public float nodeSampleDistance = 1.5f;

    // Cached node IDs on each side of the doorway
    private int nodeA = -1;
    private int nodeB = -1;

    // Current state
    private bool isClosed = false;

    void Start()
{
    // Wait one frame for GraphManager.Start() to finish building nodes
    StartCoroutine(InitAfterGraph());
}

private System.Collections.IEnumerator InitAfterGraph()
{
    yield return null; // wait one frame
    ResolveNodes();

    if (startClosed)
        CloseDoor();
}

    // -----------------------------------------------------------------------
    // Finds the two graph nodes on either side of this door
    // Uses manualNodeA/B if set, otherwise auto-detects via NearestNode()
    // -----------------------------------------------------------------------
    void ResolveNodes()
    {
        if (manualNodeA != -1 && manualNodeB != -1)
        {
            nodeA = manualNodeA;
            nodeB = manualNodeB;
            Debug.Log($"[DoorInteractable] Using manual node IDs: {nodeA} <-> {nodeB}");
            return;
        }

        if (graphManager == null)
        {
            Debug.LogError("[DoorInteractable] GraphManager not assigned!");
            return;
        }

        // Sample in front of and behind the door using its local forward axis
        Vector3 frontPos = transform.position + transform.forward * nodeSampleDistance;
        Vector3 backPos  = transform.position - transform.forward * nodeSampleDistance;

        nodeA = graphManager.NearestNode(frontPos);
        nodeB = graphManager.NearestNode(backPos);

        Debug.Log($"[DoorInteractable] Auto-detected nodes: {nodeA} (front) <-> {nodeB} (back)");
    }

    // -----------------------------------------------------------------------
    // CLOSE DOOR — severs graph edge through the doorway
    // Called by Systems Engineer's door animation script (or PlayerInteraction.cs)
    // -----------------------------------------------------------------------
    public void CloseDoor()
    {
        if (isClosed) return;
        if (nodeA == -1 || nodeB == -1 || nodeA == nodeB)
        {
            Debug.LogWarning("[DoorInteractable] Cannot sever edge — invalid node IDs.");
            return;
        }

        isClosed = true;

        // === CORE IS2 ACTION: Sever the doorway edge ===
        graphManager.RemoveEdge(nodeA, nodeB);

        Debug.Log($"[DoorInteractable] Door CLOSED at {transform.position}. " +
                  $"Edge severed: node {nodeA} <-> node {nodeB}.");

        if (interceptor != null)
            interceptor.OnEdgeSevered(nodeA, nodeB, this.gameObject);
    }

    // -----------------------------------------------------------------------
    // OPEN DOOR — restores graph edge through the doorway
    // -----------------------------------------------------------------------
    public void OpenDoor()
    {
        if (!isClosed) return;

        isClosed = false;

        Debug.Log($"[DoorInteractable] Door OPENED at {transform.position}. " +
                  $"Restoring edge: node {nodeA} <-> node {nodeB}.");

        if (interceptor != null)
            interceptor.OnEdgeRestored(nodeA, nodeB, this.gameObject);
    }

    // -----------------------------------------------------------------------
    // Toggle — convenience method for a single interact button
    // -----------------------------------------------------------------------
    public void ToggleDoor()
    {
        if (isClosed) OpenDoor();
        else          CloseDoor();
    }

    public bool IsClosed => isClosed;

    void OnDrawGizmosSelected()
    {
        // Visualize the sample points used to find nodes
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position + transform.forward * nodeSampleDistance, 0.25f);
        Gizmos.DrawSphere(transform.position - transform.forward * nodeSampleDistance, 0.25f);
        Gizmos.DrawLine(
            transform.position + transform.forward * nodeSampleDistance,
            transform.position - transform.forward * nodeSampleDistance);
    }
}
