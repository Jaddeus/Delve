using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public float mouseSensitivity = 2f;
    public float maxLookUpAngle = 90f;
    public float maxLookDownAngle = 90f;
    public float repairRange = 5f;

    private float xRotation = 0f;
    private Transform playerTransform;
    private Camera cam;
    private BoltRepair currentTarget;

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
        HandleRepairInput();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate the player horizontally
        playerTransform.Rotate(Vector3.up * mouseX);

        // Rotate the camera vertically
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookDownAngle, maxLookUpAngle);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleRepairInput()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, repairRange))
        {
            BoltRepair boltRepair = hit.collider.GetComponent<BoltRepair>();
            if (boltRepair != null)
            {
                if (currentTarget != boltRepair)
                {
                    if (currentTarget != null)
                    {
                        currentTarget.SetTargeted(false);
                    }
                    currentTarget = boltRepair;
                    currentTarget.SetTargeted(true);
                }
            }
            else
            {
                if (currentTarget != null)
                {
                    currentTarget.SetTargeted(false);
                    currentTarget = null;
                }
            }
        }
        else
        {
            if (currentTarget != null)
            {
                currentTarget.SetTargeted(false);
                currentTarget = null;
            }
        }
    }
}