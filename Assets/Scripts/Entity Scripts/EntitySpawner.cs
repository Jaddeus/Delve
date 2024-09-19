using UnityEngine;

public class EntitySpawner : MonoBehaviour
{
    public GameObject entityPrefab;
    public Vector3 spawnPosition;

    public void SpawnEntity()
    {
        if (entityPrefab != null)
        {
            GameObject spawnedEntity = Instantiate(entityPrefab, spawnPosition, Quaternion.identity);
            Entity entity = spawnedEntity.GetComponent<Entity>();
            if (entity != null)
            {
                entity.Initialize(spawnPosition);
            }
            else
            {
                Debug.LogWarning("Spawned prefab does not have an Entity component.");
            }
        }
        else
        {
            Debug.LogError("Entity prefab is not assigned to the EntitySpawner.");
        }
    }
}