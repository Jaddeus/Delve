using UnityEngine;

// DUPLICATE THIS AND MODIFY FOR EACH NEW STAMINA DRAINING OBJECT/INTERACTION

public class NewStaminaDrainer : MonoBehaviour, IStaminaDrainer
{
    public float staminaDrainRate = 5f; // Amount of stamina drained per second
    public float minStaminaRequired = 2f; // Minimum stamina required to continue the interaction
    private bool isInteracting = false;

    // Implement IStaminaDrainer interface methods
    public float GetStaminaDrainRate()
    {
        return staminaDrainRate;
    }

    public bool IsInteracting()
    {
        return isInteracting;
    }

    // Method to check current stamina to see if the interaction can start
    public bool CanStartInteraction(float currentStamina)
    {
        // Allow interaction to start if there's any stamina available
        return currentStamina > 0;
    }

    // Method to start interaction
    public void StartInteraction()
    {
        isInteracting = true;
    }

    // Method to stop interaction
    public void StopInteraction()
    {
        isInteracting = false;
    }

    // Add any other necessary methods or logic for your new drainer
}