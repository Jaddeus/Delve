using UnityEngine;

public class PickupableObject : MonoBehaviour
{
    // Material to use when the object is targeted for pickup
    public Material highlightMaterial;
    // Default material of the object
    public Material defaultMaterial;
    // Minimum vertical offset from the ground level
    public float minYOffset = 0.5f; // Offset from the ground level

    // Flag to indicate if the object is currently targeted
    private bool isTargeted = false;
    // Flag to indicate if the object is currently picked up
    private bool isPickedUp = false;
    // Reference to the object's renderer component
    private Renderer objectRenderer;
    // Reference to the object's rigidbody component
    private Rigidbody rb;
    // Minimum Y position to prevent falling through the ground
    private float minY;

    private void Start()
    {
        // Get the Renderer component
        objectRenderer = GetComponent<Renderer>();
        // Get or add a Rigidbody component
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        // Enable gravity and disable kinematic mode
        rb.useGravity = true;
        rb.isKinematic = false;

        // Calculate the minimum Y position
        minY = FindGroundLevel() + minYOffset;
    }

    private void Update()
    {
        // Prevent falling through the map
        if (transform.position.y < minY)
        {
            // Adjust position to be at least at minY
            Vector3 newPosition = transform.position;
            newPosition.y = minY;
            transform.position = newPosition;
            // Stop any downward movement
            rb.velocity = Vector3.zero;
        }
    }

    private float FindGroundLevel()
    {
        RaycastHit hit;
        // Cast a ray downwards to find the ground
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity))
        {
            return hit.point.y;
        }
        return 0f; // Default to 0 if no ground is found
    }

    public void SetTargeted(bool targeted)
    {
        // Update the targeted state
        isTargeted = targeted;
        // Change material based on targeted state
        objectRenderer.material = targeted ? highlightMaterial : defaultMaterial;
    }

    public void Pickup(Transform camera)
    {
        // Set picked up state
        isPickedUp = true;
        // Disable gravity and enable kinematic mode when picked up
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    public void Drop(Vector3 dropPosition)
    {
        // Reset picked up state
        isPickedUp = false;
        // Re-enable gravity and disable kinematic mode
        rb.useGravity = true;
        rb.isKinematic = false;
        // Set the drop position
        transform.position = dropPosition;
        // Reset any existing velocity
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public bool IsPickedUp()
    {
        // Return the current picked up state
        return isPickedUp;
    }
}