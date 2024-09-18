using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public float mouseSensitivity = 2f;
    public float maxLookUpAngle = 90f;
    public float maxLookDownAngle = 90f;
    public float interactionRange = 5f;
    public float pickupDistance = 1f;
    public float pickupRightOffset = 0.5f;

    private float xRotation = 0f;
    private Transform playerTransform;
    private Camera cam;
    private BoltRepair currentBoltTarget;
    private PickupableObject currentPickupTarget;
    private PickupableObject pickedUpObject;

    private void Start()
    {
        playerTransform = transform.parent;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        HandleMouseLook();
        HandleInteraction();

        if (pickedUpObject != null)
        {
            UpdatePickedUpObjectPosition();
        }

        // Handle drop input regardless of highlighting
        if (Input.GetKeyDown(KeyCode.G) && pickedUpObject != null)
        {
            DropObject();
        }
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookDownAngle, maxLookUpAngle);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerTransform.Rotate(Vector3.up * mouseX);
    }

    private void HandleInteraction()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            BoltRepair boltRepair = hit.collider.GetComponent<BoltRepair>();
            PickupableObject pickupable = hit.collider.GetComponent<PickupableObject>();

            if (boltRepair != null)
            {
                HandleBoltRepairInteraction(boltRepair);
            }
            else if (pickupable != null)
            {
                HandlePickupInteraction(pickupable);
            }
            else
            {
                ClearInteractions();
            }
        }
        else
        {
            ClearInteractions();
        }
    }

    private void HandleBoltRepairInteraction(BoltRepair boltRepair)
    {
        if (currentBoltTarget != boltRepair)
        {
            if (currentBoltTarget != null)
            {
                currentBoltTarget.SetTargeted(false);
            }
            currentBoltTarget = boltRepair;
            currentBoltTarget.SetTargeted(true);
        }

        if (currentPickupTarget != null)
        {
            currentPickupTarget.SetTargeted(false);
            currentPickupTarget = null;
        }
    }

    private void HandlePickupInteraction(PickupableObject pickupable)
    {
        if (currentPickupTarget != pickupable)
        {
            if (currentPickupTarget != null)
            {
                currentPickupTarget.SetTargeted(false);
            }
            currentPickupTarget = pickupable;
            currentPickupTarget.SetTargeted(true);
        }

        if (currentBoltTarget != null)
        {
            currentBoltTarget.SetTargeted(false);
            currentBoltTarget = null;
        }

        if (Input.GetKeyDown(KeyCode.E) && pickedUpObject == null)
        {
            PickupObject(pickupable);
        }
    }

    private void ClearInteractions()
    {
        if (currentBoltTarget != null)
        {
            currentBoltTarget.SetTargeted(false);
            currentBoltTarget = null;
        }

        if (currentPickupTarget != null)
        {
            currentPickupTarget.SetTargeted(false);
            currentPickupTarget = null;
        }
    }

    private void UpdatePickedUpObjectPosition()
    {
        Vector3 targetPosition = transform.position + transform.forward * pickupDistance + transform.right * pickupRightOffset;
        pickedUpObject.transform.position = Vector3.Lerp(pickedUpObject.transform.position, targetPosition, Time.deltaTime * 10f);
        pickedUpObject.transform.rotation = Quaternion.Slerp(pickedUpObject.transform.rotation, transform.rotation, Time.deltaTime * 10f);
    }

    private void PickupObject(PickupableObject pickupable)
    {
        pickedUpObject = pickupable;
        pickedUpObject.Pickup(transform);
    }

    private void DropObject()
    {
        Vector3 dropPosition = transform.position + transform.forward * 2f;
        pickedUpObject.Drop(dropPosition);
        pickedUpObject = null;
    }

    public bool IsCarryingObject()
    {
        return pickedUpObject != null;
    }
}