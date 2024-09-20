using UnityEngine;
using System.Collections.Generic; // Add this line to import List<T>

public class PlayerMovement : MonoBehaviour
{
    // Maximum speed of the player
    public float maxSpeed = 5f;
    // Rate at which the player accelerates
    public float acceleration = 20f;
    // Rate at which the player decelerates
    public float deceleration = 10f;
    // Speed multiplier when carrying an object
    public float carryingSpeedMultiplier = 0.7f; // Reduce speed when carrying an object
    // Maximum health of the player
    public float maxHealth = 100f;
    // Current health of the player
    private float currentHealth;
    // Current speed of the player, visible in the inspector
    [SerializeField] private float currentSpeed;
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f; // Amount of stamina drained per second while sprinting
    public float staminaRegenRate = 10f; // Amount of stamina regenerated per second when not sprinting
    public float sprintSpeedMultiplier = 1.5f; // Speed multiplier when sprinting
    public float minStaminaToSprint = 1f; // Minimum stamina required to start sprinting
    public float currentStamina; // Amount of stamina the player currently has
    private bool isSprinting = false; // Sprint state
    // List to keep track of objects currently draining stamina
    private List<IStaminaDrainer> currentStaminaDrainers = new List<IStaminaDrainer>();
    public float staminaRecoveryCooldown = 2f; // Time in seconds before stamina starts regenerating after hitting 0
    private float staminaRecoveryTimer = 0f; // Timer to track cooldown
    private bool isStaminaDepleted = false; // Flag to check if stamina was recently depleted

    // Reference to the CharacterController component
    private CharacterController controller;
    // Horizontal and forward/backward velocity
    private Vector3 velocity;
    // Vertical (up/down) velocity
    private Vector3 verticalVelocity;
    // Reference to the camera's transform
    private Transform cameraTransform;
    // Reference to the FirstPersonCamera script
    private FirstPersonCamera fpCamera;

    private void Start()
    {
        // Get the CharacterController component attached to this GameObject
        controller = GetComponent<CharacterController>();
        // Find the Camera in the children of this GameObject and get its transform
        cameraTransform = GetComponentInChildren<Camera>().transform;
        // Get the FirstPersonCamera script attached to the camera
        fpCamera = cameraTransform.GetComponent<FirstPersonCamera>();
        // Initialize the player's health to maximum
        currentHealth = maxHealth;
        currentStamina = maxStamina; // Initialize stamina to max value
    }

    private void Update()
    {
        // Handle WASD movement input
        HandleWASDMovement();
        // Handle vertical (up/down) movement input
        HandleVerticalMovement();
        // Handle sprinting
        HandleSprinting();
        // Update stamina
        UpdateStamina();
        // Handle stamina drain from interactable objects
        HandleStaminaDrainers();

        // Combine horizontal/forward and vertical velocities
        Vector3 combinedVelocity = velocity + verticalVelocity;

        // Apply speed reduction if carrying an object and sprint speed multiplier if sprinting
        float speedMultiplier = 1f;
        if (fpCamera.IsCarryingObject())
        {
            speedMultiplier *= carryingSpeedMultiplier;
        }
        if (isSprinting)
        {
            speedMultiplier *= sprintSpeedMultiplier;
        }

        // Limit the combined velocity to the maximum speed (with sprint and carrying multipliers)
        if (combinedVelocity.magnitude > maxSpeed * speedMultiplier)
        {
            combinedVelocity = combinedVelocity.normalized * maxSpeed * speedMultiplier;
        }

        // Update the current speed for display or other uses
        currentSpeed = combinedVelocity.magnitude;

        // Move the player using the CharacterController
        controller.Move(combinedVelocity * Time.deltaTime);
    }

    private void HandleWASDMovement()
    {
        // Get raw input for horizontal and vertical axes
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        // Calculate the input direction relative to the camera
        Vector3 inputDirection = CalculateInputDirection(moveHorizontal, moveVertical);

        // Calculate the current speed multiplier
        float speedMultiplier = 1f;
        if (fpCamera.IsCarryingObject())
        {
            speedMultiplier *= carryingSpeedMultiplier;
        }
        if (isSprinting)
        {
            speedMultiplier *= sprintSpeedMultiplier;
        }

        // If there's significant input, accelerate towards the input direction
        if (inputDirection.magnitude > 0.1f)
        {
            velocity = Vector3.MoveTowards(velocity, inputDirection * maxSpeed * speedMultiplier, acceleration * Time.deltaTime);
        }
        else
        {
            // If no input, decelerate towards zero
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, deceleration * Time.deltaTime);
        }
    }

    private void HandleVerticalMovement()
    {
        // Initialize target vertical velocity
        Vector3 targetVerticalVelocity = Vector3.zero;

        // Set upward velocity if Space key is pressed
        if (Input.GetKey(KeyCode.Space))
        {
            targetVerticalVelocity = Vector3.up * maxSpeed;
        }
        // Set downward velocity if Left Control key is pressed
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            targetVerticalVelocity = Vector3.down * maxSpeed;
        }

        // If there's a target vertical velocity, accelerate towards it
        if (targetVerticalVelocity != Vector3.zero)
        {
            verticalVelocity = Vector3.MoveTowards(verticalVelocity, targetVerticalVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            // If no vertical input, decelerate towards zero
            verticalVelocity = Vector3.MoveTowards(verticalVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }
    }

    private void HandleSprinting()
    {
        // Check if the player is pressing the sprint key (left shift) and has enough stamina
        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > minStaminaToSprint && !isStaminaDepleted)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
    }

    private void UpdateStamina()
    {
        if (isSprinting)
        {
            // Drain stamina while sprinting
            DrainStamina(staminaDrainRate * Time.deltaTime);
            Debug.Log($"Sprinting. Current stamina: {currentStamina}");
        }
    }

    // Method to drain stamina for other actions (e.g., completing objectives)
    public void DrainStamina(float amount)
    {
        currentStamina = Mathf.Max(0, currentStamina - amount);
        if (currentStamina == 0 && !isStaminaDepleted)
        {
            isStaminaDepleted = true;
            staminaRecoveryTimer = 0f;
            Debug.Log("Stamina depleted, starting recovery cooldown.");
        }
    }

    public float GetCurrentStaminaPercentage()
    {
        // Method to get current stamina as a percentage (useful for UI)
        return (currentStamina / maxStamina) * 100f;
    }

    // Method to manage stamina drain from interactable objects
    private void HandleStaminaDrainers()
    {
        float totalDrain = 0f;
        bool drainersRemoved = false;

        // Iterate through the list in reverse order to safely remove items
        for (int i = currentStaminaDrainers.Count - 1; i >= 0; i--)
        {
            var drainer = currentStaminaDrainers[i];
            if (drainer != null && drainer.IsInteracting())
            {
                // Check if the interaction can continue based on current stamina
                float drainAmount = drainer.GetStaminaDrainRate() * Time.deltaTime;
                if (currentStamina - totalDrain > 0)
                {
                    totalDrain += Mathf.Min(drainAmount, currentStamina - totalDrain); ;
                }
                else
                {
                    // If not enough stamina, stop the interaction
                    (drainer as MonoBehaviour)?.SendMessage("StopInteraction", SendMessageOptions.DontRequireReceiver);
                }
            }
            else
            {
                currentStaminaDrainers.RemoveAt(i);
                drainersRemoved = true;
            }
        }

        // Apply the total stamina drain for this frame
        if (totalDrain > 0)
        {
            Debug.Log($"Draining stamina from interactables: {totalDrain}");
            DrainStamina(totalDrain);
        }
        else if (!isSprinting && currentStaminaDrainers.Count == 0)
        {
            // Regenerate stamina when not draining, not sprinting, and no active drainers
            RegenerateStamina();
        }

        if (drainersRemoved)
        {
            Debug.Log($"Removed inactive drainers. Current drainer count: {currentStaminaDrainers.Count}");
        }
    }

    // Method to handle stamina regeneration
    private void RegenerateStamina()
    {
        if (isStaminaDepleted)
        {
            staminaRecoveryTimer += Time.deltaTime;
            if (staminaRecoveryTimer >= staminaRecoveryCooldown)
            {
                isStaminaDepleted = false;
                staminaRecoveryTimer = 0f;
            }
            else
            {
                return; // Don't regenerate stamina during cooldown
            }
        }
        if (currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
            Debug.Log($"Regenerating stamina. Current stamina: {currentStamina}");
        }
    }

    // Method to add a new stamina drainer
    public void AddStaminaDrainer(IStaminaDrainer drainer)
    {
        // Only add the drainer if it's not already in the list
        if (!currentStaminaDrainers.Contains(drainer))
        {
            currentStaminaDrainers.Add(drainer);
        }
    }

    // Method to remove a stamina drainer
    public void RemoveStaminaDrainer(IStaminaDrainer drainer)
    {
        if (currentStaminaDrainers.Remove(drainer))
        {
            Debug.Log($"Removed stamina drainer. Current drainer count: {currentStaminaDrainers.Count}");
        }
    }

    private Vector3 CalculateInputDirection(float moveHorizontal, float moveVertical)
    {
        // Get the forward and right vectors of the camera
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Calculate and normalize the input direction relative to the camera
        return (forward * moveVertical + right * moveHorizontal).normalized;
    }

    public void TakeDamage(float damage)
    {
        // Reduce the player's health by the damage amount
        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");

        // Check if the player's health has dropped to or below zero
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        // Implement death behavior (e.g., respawn, game over screen, etc.)
    }
}