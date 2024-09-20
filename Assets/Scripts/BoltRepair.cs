using UnityEngine;

// This script manages the repair behavior of bolts in the game.
// It allows players to repair (shrink) bolts by targeting and clicking on them.
public class BoltRepair : MonoBehaviour, IStaminaDrainer
{
    // Speed at which the bolt shrinks when being repaired
    public float repairSpeed = 0.5f;
    // Minimum size of the bolt before it's considered fully repaired and destroyed
    public float minSize = 0.1f;
    // Material to use when the bolt is targeted for repair
    public Material highlightMaterial;
    // Default material of the bolt
    public Material defaultMaterial;
    // Indicates whether the bolt is currently targeted for repair
    private bool isTargeted = false;
    // Reference to the bolt's renderer component
    private Renderer objectRenderer;
    // Original scale of the bolt, used to calculate shrinking
    private Vector3 originalScale;
    // Determines if this specific bolt should drain stamina
    public bool drainStamina = false;
    // Rate at which stamina is drained per second when interacting
    public float staminaDrainRate = 10f;
    public float minStaminaRequired = 5f; // Minimum stamina required to continue repairing
    // Tracks whether the player is currently interacting with this bolt
    private bool isInteracting = false;
    private PlayerMovement playerMovement; // Giving BoltRepair access to playerMovement

    // Initialize component references and store the original scale
    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        originalScale = transform.localScale;
        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement not found in the scene. BoltRepair will not function correctly.");
        }
    }

    private void Update()
    {
        // If the player is targeting this bolt and holding the mouse button, repair it
        if (isTargeted && Input.GetMouseButton(0)) // Left mouse button
        {
            Repair();
        }
        else if (isInteracting)
        {
            // If the player was interacting but stopped, call StopInteraction
            StopInteraction();
        }

        // Destroy the bolt if it's fully repaired
        if (transform.localScale.x <= minSize)
        {
            Destroy(gameObject);
        }
    }

    // Called by other scripts (likely FirstPersonCamera) to set the targeted state
    public void SetTargeted(bool targeted)
    {
        isTargeted = targeted;
        objectRenderer.material = targeted ? highlightMaterial : defaultMaterial;
    }

    public bool CanStartInteraction(float currentStamina)
    {
        // Check if the player has enough stamina to continue repairing
        return currentStamina > 0;
    }

    // Shrink the bolt when being repaired
    private void Repair()
    {
        if (playerMovement != null && playerMovement.currentStamina > 0)
        {
            isInteracting = true;
            Vector3 newScale = transform.localScale - originalScale * (repairSpeed * Time.deltaTime);
            transform.localScale = Vector3.Max(newScale, originalScale * minSize);

            // If the bolt is fully repaired, stop interaction
            if (transform.localScale.x <= minSize * originalScale.x)
            {
                StopInteraction();
            }
        }
        else
        {
            StopInteraction();
        }
    }

    // Method to stop interaction
    public void StopInteraction()
    {
        isInteracting = false;
    }

    private void OnDestroy()
    {
        StopInteraction();
    }

    // Ensure StopInteraction is called when the object is destroyed
    public float GetStaminaDrainRate()
    {
        // Return the drain rate if stamina drain is enabled, otherwise return 0
        return drainStamina ? staminaDrainRate : 0f;
    }

    public bool IsInteracting()
    {
        return isInteracting;
    }

}