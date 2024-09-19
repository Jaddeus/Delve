using UnityEngine;

public class Entity : MonoBehaviour
{
    // Name of the entity
    public string entityName;
    // Current health of the entity
    public float health = 100f;

    // Virtual method called when the entity is created
    protected virtual void Start()
    {
        // Log the entity's creation with its name and initial health
        Debug.Log($"Entity '{entityName}' spawned with {health} health.");
    }

    // Virtual method to initialize the entity at a specific position
    public virtual void Initialize(Vector3 position)
    {
        // Set the entity's position in the world
        transform.position = position;
    }

    // Add more entity-specific methods here as needed
}