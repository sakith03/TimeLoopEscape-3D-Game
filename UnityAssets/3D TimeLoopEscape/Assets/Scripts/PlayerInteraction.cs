using UnityEngine;


public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float pickupRange = 2.5f;
    public float holdDistance = 2f;
    public LayerMask interactableLayer;
    public KeyCode interactKey = KeyCode.E;

    [Header("Physics - Carrying")]
    public float carryForce = 800f;
    public float carryTorque = 100f;

    // Currently held object
    private GameObject heldObject;
    private BarricadeInteractable heldBarricade;
    private Rigidbody heldRb;

    // Camera reference for direction
    private Camera playerCamera;

    void Awake()
    {
        playerCamera = Camera.main;
    }

    void Update()
    {
        // E key: pick up or drop
        if (Input.GetKeyDown(interactKey))
        {
            if (heldObject != null)
                DropHeldObject();
            else
                TryPickup();
        }

        // F key: interact with door (toggle)
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryInteractDoor();
        }
    }

    void FixedUpdate()
    {
        // If holding an object, pull it toward the hold point
        if (heldObject != null && heldRb != null)
        {
            Vector3 holdPoint = playerCamera.transform.position
                              + playerCamera.transform.forward * holdDistance;

            Vector3 delta = holdPoint - heldObject.transform.position;
            heldRb.linearVelocity = delta * (carryForce * Time.fixedDeltaTime);
        }
    }

    // -----------------------------------------------------------------------
    // PICK UP
    // -----------------------------------------------------------------------
    void TryPickup()
    {
        Ray ray = playerCamera.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f));

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, interactableLayer))
        {
            BarricadeInteractable barricade = hit.collider.GetComponent<BarricadeInteractable>();
            if (barricade != null)
            {
                PickupBarricade(barricade);
                return;
            }
        }
    }

    void PickupBarricade(BarricadeInteractable barricade)
    {
        heldObject   = barricade.gameObject;
        heldBarricade = barricade;
        heldRb       = heldObject.GetComponent<Rigidbody>();

        if (heldRb != null)
        {
            heldRb.useGravity = false;
            heldRb.linearDamping = 10f;
        }

        // === IS2 HOOK — notify graph that this barricade is no longer blocking ===
        barricade.OnBarricadeRemoved();
    }

    // -----------------------------------------------------------------------
    // DROP
    // -----------------------------------------------------------------------
    void DropHeldObject()
    {
        if (heldRb != null)
        {
            heldRb.useGravity = true;
            heldRb.linearDamping = 0f;

            // Give it a small forward toss
            heldRb.linearVelocity = playerCamera.transform.forward * 3f;
        }

        // === IS2 HOOK — mark as thrown so BarricadeInteractable auto-triggers
        //                placement detection when it settles (via FixedUpdate) ===
        if (heldBarricade != null)
            heldBarricade.MarkAsThrown();

        heldObject    = null;
        heldBarricade = null;
        heldRb        = null;
    }

    // -----------------------------------------------------------------------
    // DOOR TOGGLE
    // -----------------------------------------------------------------------
    void TryInteractDoor()
    {
        Ray ray = playerCamera.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f));

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, interactableLayer))
        {
            DoorInteractable door = hit.collider.GetComponent<DoorInteractable>();
            if (door != null)
            {
                // === IS2 HOOK — toggles door and severs/restores graph edge ===
                door.ToggleDoor();
            }
        }
    }
}
