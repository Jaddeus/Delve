using UnityEngine;

public class Entity : MonoBehaviour
{
    public string entityName;
    public float health = 100f;

    protected virtual void Start()
    {
        Debug.Log($"Entity '{entityName}' spawned with {health} health.");
    }

    public virtual void Initialize(Vector3 position)
    {
        transform.position = position;
    }

    // Add more entity-specific methods here as needed
}