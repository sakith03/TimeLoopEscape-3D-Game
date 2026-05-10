using UnityEngine;



public class BarricadeInteractable : MonoBehaviour
{
    [Header("Graph References")]
    [Tooltip("Drag the GraphManager GameObject here")]
    public GraphManager graphManager;

    [Tooltip("Drag the EnvironmentEventInterceptor GameObject here")]
    public EnvironmentEventInterceptor interceptor;

    [Header("Detection Settings")]
    [Tooltip("How far to search for blocked nodes on each side of the barricade")]
    public float detectionRadius = 3f;

    [Tooltip("Layer for the barricade itself - used to avoid self-raycast")]
    public LayerMask barricadeLayer;

    // Tracks the two nodes this barricade is currently blocking
    // Stored so RestoreEdge() can reverse it if barricade is removed
    private int blockedNodeA = -1;
    private int blockedNodeB = -1;

    // Whether this barricade has an active edge severance
    private bool isBlocking = false;

    // -----------------------------------------------------------------------
    // Called by PlayerInteraction.cs (Systems Engineer) when player DROPS
    // the barricade and it settles. Wire this up via UnityEvent or direct call.
    // -----------------------------------------------------------------------
    public void OnBarricadePlaced()
    {
        if (graphManager == null)
        {
            Debug.LogError("[BarricadeInteractable] GraphManager reference not set!");
            return;
        }

        // Find the two nearest graph nodes on either side of this barricade
        // Strategy: sample slightly left and right of the barricade's forward axis
        Vector3 posA = transform.position + transform.right * detectionRadius * 0.5f;
        Vector3 posB = transform.position - transform.right * detectionRadius * 0.5f;

        int nodeA = graphManager.NearestNode(posA);
        int nodeB = graphManager.NearestNode(posB);

        // Safety check - if both sides resolve to the same node, the barricade
        // is too small or poorly placed; do nothing
        if (nodeA == nodeB || nodeA == -1 || nodeB == -1)
        {
            Debug.LogWarning($"[BarricadeInteractable] Could not find two distinct nodes " +
                             $"around barricade at {transform.position}. Edge not severed.");
            return;
        }

        // Store for later restoration
        blockedNodeA = nodeA;
        blockedNodeB = nodeB;
        isBlocking = true;

        // === CORE IS2 ACTION: Sever the edge in the mathematical graph ===
        graphManager.RemoveEdge(nodeA, nodeB);

        Debug.Log($"[BarricadeInteractable] Barricade placed at {transform.position}. " +
                  $"Severed edge between node {nodeA} and node {nodeB}.");

        // Notify interceptor to trigger agent path recalculation
        if (interceptor != null)
            interceptor.OnEdgeSevered(nodeA, nodeB, this.gameObject);
    }

    // -----------------------------------------------------------------------
    // Called by PlayerInteraction.cs when the player PICKS UP / removes the
    // barricade. Restores the edge in the graph.
    // -----------------------------------------------------------------------
    public void OnBarricadeRemoved()
    {
        if (!isBlocking || graphManager == null) return;

        // Restore the severed edge so agents can use that path again
        // GraphManager doesn't have RestoreEdge() - we use the interceptor's
        // cached edge list to rebuild it
        if (interceptor != null)
            interceptor.OnEdgeRestored(blockedNodeA, blockedNodeB, this.gameObject);

        Debug.Log($"[BarricadeInteractable] Barricade removed. " +
                  $"Restoring edge between node {blockedNodeA} and node {blockedNodeB}.");

        blockedNodeA = -1;
        blockedNodeB = -1;
        isBlocking = false;
    }

    // -----------------------------------------------------------------------
    // Physics fallback: if the barricade has a Rigidbody and is thrown/dropped,
    // detect when it stops moving and auto-trigger placement
    // -----------------------------------------------------------------------
    private Rigidbody rb;
    private bool hasSettled = false;
    private bool wasThrown = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Call this from PlayerInteraction when the player THROWS the barricade
    public void MarkAsThrown()
    {
        wasThrown = true;
        hasSettled = false;
    }

    void FixedUpdate()
    {
        // If it was thrown and has now come to rest, trigger placement
        if (wasThrown && !hasSettled && rb != null)
        {
            if (rb.linearVelocity.magnitude < 0.05f && rb.angularVelocity.magnitude < 0.05f)
            {
                hasSettled = true;
                wasThrown = false;
                OnBarricadePlaced();
            }
        }
    }

    // Draw detection zones in Scene view for debugging
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position + transform.right * detectionRadius * 0.5f, 0.3f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(
            transform.position - transform.right * detectionRadius * 0.5f, 0.3f);

        Gizmos.color = new Color(1, 0.5f, 0, 0.4f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
