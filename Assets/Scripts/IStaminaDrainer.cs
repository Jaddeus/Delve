// IStaminaDrainer.cs
using UnityEngine;

// This interface defines the contract for objects that can drain stamina
public interface IStaminaDrainer
{
    // Returns the rate at which stamina should be drained per second
    float GetStaminaDrainRate();

    // Indicates whether the object is currently being interacted with
    bool IsInteracting();
}