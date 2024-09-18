using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float maxSpeed = 5f;
    public float acceleration = 20f;
    public float deceleration = 10f;
    public float carryingSpeedMultiplier = 0.7f; // Reduce speed when carrying an object

    [SerializeField] private float currentSpeed;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 verticalVelocity;
    private Transform cameraTransform;
    private FirstPersonCamera fpCamera;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = GetComponentInChildren<Camera>().transform;
        fpCamera = cameraTransform.GetComponent<FirstPersonCamera>();
    }

    private void Update()
    {
        HandleWASDMovement();
        HandleVerticalMovement();

        Vector3 combinedVelocity = velocity + verticalVelocity;

        float speedMultiplier = fpCamera.IsCarryingObject() ? carryingSpeedMultiplier : 1f;

        if (combinedVelocity.magnitude > maxSpeed * speedMultiplier)
        {
            combinedVelocity = combinedVelocity.normalized * maxSpeed * speedMultiplier;
        }

        currentSpeed = combinedVelocity.magnitude;

        controller.Move(combinedVelocity * Time.deltaTime);
    }

    private void HandleWASDMovement()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = CalculateInputDirection(moveHorizontal, moveVertical);

        if (inputDirection.magnitude > 0.1f)
        {
            velocity = Vector3.MoveTowards(velocity, inputDirection * maxSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, deceleration * Time.deltaTime);
        }
    }

    private void HandleVerticalMovement()
    {
        Vector3 targetVerticalVelocity = Vector3.zero;

        if (Input.GetKey(KeyCode.Space))
        {
            targetVerticalVelocity = Vector3.up * maxSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            targetVerticalVelocity = Vector3.down * maxSpeed;
        }

        if (targetVerticalVelocity != Vector3.zero)
        {
            verticalVelocity = Vector3.MoveTowards(verticalVelocity, targetVerticalVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            verticalVelocity = Vector3.MoveTowards(verticalVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }
    }

    private Vector3 CalculateInputDirection(float moveHorizontal, float moveVertical)
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        return (forward * moveVertical + right * moveHorizontal).normalized;
    }
}