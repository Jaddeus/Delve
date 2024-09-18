using UnityEngine;

public class PickupableObject : MonoBehaviour
{
    public Material highlightMaterial;
    public Material defaultMaterial;
    public float minYOffset = 0.5f; // Offset from the ground level

    private bool isTargeted = false;
    private bool isPickedUp = false;
    private Renderer objectRenderer;
    private Rigidbody rb;
    private float minY;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = true;
        rb.isKinematic = false;

        // Set the minY based on the ground level and offset
        minY = FindGroundLevel() + minYOffset;
    }

    private void Update()
    {
        // Prevent falling through the map
        if (transform.position.y < minY)
        {
            Vector3 newPosition = transform.position;
            newPosition.y = minY;
            transform.position = newPosition;
            rb.velocity = Vector3.zero;
        }
    }

    private float FindGroundLevel()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity))
        {
            return hit.point.y;
        }
        return 0f; // Default to 0 if no ground is found
    }

    public void SetTargeted(bool targeted)
    {
        isTargeted = targeted;
        objectRenderer.material = targeted ? highlightMaterial : defaultMaterial;
    }

    public void Pickup(Transform camera)
    {
        isPickedUp = true;
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    public void Drop(Vector3 dropPosition)
    {
        isPickedUp = false;
        rb.useGravity = true;
        rb.isKinematic = false;
        transform.position = dropPosition;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public bool IsPickedUp()
    {
        return isPickedUp;
    }
}