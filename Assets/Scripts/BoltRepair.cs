using UnityEngine;

public class BoltRepair : MonoBehaviour
{
    public float repairSpeed = 0.5f;
    public float minSize = 0.1f;
    public Material highlightMaterial;
    public Material defaultMaterial;

    private bool isTargeted = false;
    private Renderer objectRenderer;
    private Vector3 originalScale;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        originalScale = transform.localScale;
    }

    private void Update()
    {
        if (isTargeted && Input.GetMouseButton(0)) // Left mouse button
        {
            Repair();
        }

        if (transform.localScale.x <= minSize)
        {
            Destroy(gameObject);
        }
    }

    public void SetTargeted(bool targeted)
    {
        isTargeted = targeted;
        objectRenderer.material = targeted ? highlightMaterial : defaultMaterial;
    }

    private void Repair()
    {
        Vector3 newScale = transform.localScale - originalScale * (repairSpeed * Time.deltaTime);
        transform.localScale = Vector3.Max(newScale, originalScale * minSize);
    }
}