using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    // Mouse sensitivity for camera rotation
    public float mouseSensitivity = 2f;
    // Maximum angle the camera can look up
    public float maxLookUpAngle = 90f;
    // Maximum angle the camera can look down
    public float maxLookDownAngle = 90f;
    // Maximum distance for interacting with objects
    public float interactionRange = 5f;
    // Distance at which picked up objects are held
    public float pickupDistance = 1f;
    // Horizontal offset for held objects
    public float pickupRightOffset = 0.5f;

    // Stores the vertical rotation of the camera
    private float xRotation = 0f;
    // Reference to the player's transform
    private Transform playerTransform;
    // Reference to the camera component
    private Camera cam;
    // Reference to the currently targeted bolt for repair
    private BoltRepair currentBoltTarget;
    // Reference to the currently targeted pickupable object
    private PickupableObject currentPickupTarget;
    // Reference to the currently picked up object
    private PickupableObject pickedUpObject;

    private void Start()
    {
        // Set the player transform to the parent of this camera
        playerTransform = transform.parent;
        // Lock the cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Get the Camera component attached to this GameObject
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        // Handle mouse look rotation
        HandleMouseLook();
        // Handle player interactions (targeting and picking up objects)
        HandleInteraction();

        // Update the position of the picked up object if there is one
        if (pickedUpObject != null)
        {
            UpdatePickedUpObjectPosition();
        }

        // Drop the currently held object if the G key is pressed
        if (Input.GetKeyDown(KeyCode.G) && pickedUpObject != null)
        {
            DropObject();
        }
    }

    private void HandleMouseLook()
    {
        // Get mouse input for rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Calculate vertical rotation (pitch)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookDownAngle, maxLookUpAngle);

        // Apply vertical rotation to the camera
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        // Apply horizontal rotation to the player
        playerTransform.Rotate(Vector3.up * mouseX);
    }

    private void HandleInteraction()
    {
        // Cast a ray from the center of the screen
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        PlayerMovement playerMovement = GetComponentInParent<PlayerMovement>();

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            BoltRepair boltRepair = hit.collider.GetComponent<BoltRepair>();
            //USE THE FORMAT BELOW FOR NEW DRAINER IMPLEMENTATION
            //NewStaminaDrainer newDrainer = hit.collider.GetComponent<NewStaminaDrainer>();
            PickupableObject pickupable = hit.collider.GetComponent<PickupableObject>();

            if (boltRepair != null)
            {
                HandleBoltRepairInteraction(boltRepair);

                // Handle stamina draining functionality
                if (playerMovement != null)
                {
                    if (boltRepair.drainStamina && Input.GetMouseButton(0)) // Left mouse button
                    {
                        playerMovement.AddStaminaDrainer(boltRepair);
                    }
                    else
                    {
                        playerMovement.RemoveStaminaDrainer(boltRepair);
                        boltRepair.StopInteraction();
                    }
                }
            }
            //FORMAT FOR NEW DRAINER
            //else if (newDrainer != null)
            //{
            //    HandleNewDrainerInteraction(newDrainer);
            //}
            else if (pickupable != null)
            {
                HandlePickupInteraction(pickupable);
            }
            else
            {
                ClearInteractions(playerMovement);
            }
        }
        else
        {
            ClearInteractions(playerMovement);
        }
    }

    private void HandleBoltRepairInteraction(BoltRepair boltRepair)
    {
        // If targeting a new bolt, update the current target
        if (currentBoltTarget != boltRepair)
        {
            if (currentBoltTarget != null)
            {
                currentBoltTarget.SetTargeted(false);
            }
            currentBoltTarget = boltRepair;
            currentBoltTarget.SetTargeted(true);
        }

        // Clear any pickup target when targeting a bolt
        if (currentPickupTarget != null)
        {
            currentPickupTarget.SetTargeted(false);
            currentPickupTarget = null;
        }


    }

    private void HandlePickupInteraction(PickupableObject pickupable)
    {
        // If targeting a new pickupable object, update the current target
        if (currentPickupTarget != pickupable)
        {
            if (currentPickupTarget != null)
            {
                currentPickupTarget.SetTargeted(false);
            }
            currentPickupTarget = pickupable;
            currentPickupTarget.SetTargeted(true);
        }

        // Clear any bolt target when targeting a pickupable object
        if (currentBoltTarget != null)
        {
            currentBoltTarget.SetTargeted(false);
            currentBoltTarget = null;
        }

        // Pick up the object if the E key is pressed and not already carrying something
        if (Input.GetKeyDown(KeyCode.E) && pickedUpObject == null)
        {
            PickupObject(pickupable);
        }
    }

    //USE THIS FOR NEW DRAINER IMPLEMENTATION
    /*private void HandleNewDrainerInteraction(NewStaminaDrainer drainer)
    {
        PlayerMovement playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement != null)
        {
            if (Input.GetMouseButton(0)) // Left mouse button, or any other condition
            {
                drainer.StartInteraction();
                playerMovement.AddStaminaDrainer(drainer);
            }
            else
            {
                drainer.StopInteraction();
                playerMovement.RemoveStaminaDrainer(drainer);
            }
        }
    }*/

    private void ClearInteractions(PlayerMovement playerMovement)
    {
        //Clear bolt target if it exists
        if (currentBoltTarget != null)
        {
            currentBoltTarget.SetTargeted(false);
            if (playerMovement != null)
            {
                playerMovement.RemoveStaminaDrainer(currentBoltTarget);
            }
            currentBoltTarget.StopInteraction();
            currentBoltTarget = null;
        }

        // Clear pickup target if exists
        if (currentPickupTarget != null)
        {
            currentPickupTarget.SetTargeted(false);
            currentPickupTarget = null;
        }
    }

    private void UpdatePickedUpObjectPosition()
    {
        // Calculate the target position for the picked up object
        Vector3 targetPosition = transform.position + transform.forward * pickupDistance + transform.right * pickupRightOffset;
        // Smoothly move the object to the target position
        pickedUpObject.transform.position = Vector3.Lerp(pickedUpObject.transform.position, targetPosition, Time.deltaTime * 10f);
        // Smoothly rotate the object to match the camera's rotation
        pickedUpObject.transform.rotation = Quaternion.Slerp(pickedUpObject.transform.rotation, transform.rotation, Time.deltaTime * 10f);
    }

    private void PickupObject(PickupableObject pickupable)
    {
        // Set the picked up object and call its Pickup method
        pickedUpObject = pickupable;
        pickedUpObject.Pickup(transform);
    }

    private void DropObject()
    {
        // Calculate drop position in front of the player
        Vector3 dropPosition = transform.position + transform.forward * 2f;
        // Call the Drop method on the picked up object
        pickedUpObject.Drop(dropPosition);
        // Clear the reference to the picked up object
        pickedUpObject = null;
    }

    // Public method to check if the player is carrying an object
    public bool IsCarryingObject()
    {
        return pickedUpObject != null;
    }
}