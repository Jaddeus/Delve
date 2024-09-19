using UnityEngine;

public class EntitySpawner : MonoBehaviour
{
    // Prefab of the entity to be spawned
    public GameObject entityPrefab;
    // Position where the entity will be spawned
    public Vector3 spawnPosition;

    // Method to spawn a new entity
    public void SpawnEntity()
    {
        // Check if an entity prefab is assigned
        if (entityPrefab != null)
        {
            // Instantiate the entity prefab at the specified position with default rotation
            GameObject spawnedEntity = Instantiate(entityPrefab, spawnPosition, Quaternion.identity);
            // Try to get the Entity component from the spawned object
            Entity entity = spawnedEntity.GetComponent<Entity>();
            if (entity != null)
            {
                // If the Entity component exists, initialize it with the spawn position
                entity.Initialize(spawnPosition);
            }
            else
            {
                // Log a warning if the spawned prefab doesn't have an Entity component
                Debug.LogWarning("Spawned prefab does not have an Entity component.");
            }
        }
        else
        {
            // Log an error if no entity prefab is assigned to the spawner
            Debug.LogError("Entity prefab is not assigned to the EntitySpawner.");
        }
    }
}