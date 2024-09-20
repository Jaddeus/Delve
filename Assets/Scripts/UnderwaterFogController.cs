using UnityEngine;

public class UnderwaterFogController : MonoBehaviour
{
    [Tooltip("The Y position where the water surface starts")]
    public float waterSurfaceY = 0f;

    [Tooltip("The maximum depth where fog density is at its maximum")]
    public float maxDepth = -50f;

    [Tooltip("The maximum fog density at the deepest point")]
    [Range(0f, 1f)]
    public float maxFogDensity = 0.1f;

    [Tooltip("The color of the fog (water color)")]
    public Color underwaterFogColor = new Color(0, 0.4f, 0.7f, 1);

    [Header("Debug Information")]
    [SerializeField][ReadOnly] private float currentDepth;
    [SerializeField][ReadOnly] private float currentFogDensity;

    private void Start()
    {
        // Enable fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
    }

    private void Update()
    {
        // Calculate the current depth
        currentDepth = Mathf.Max(0, waterSurfaceY - transform.position.y);

        // Calculate fog density based on depth
        float depthRatio = Mathf.Clamp01(currentDepth / (waterSurfaceY - maxDepth));
        currentFogDensity = depthRatio * maxFogDensity;

        // Update fog settings
        RenderSettings.fogColor = underwaterFogColor;
        RenderSettings.fogDensity = currentFogDensity;

        // Pass information to the shader
        Shader.SetGlobalColor("_UnderwaterFogColor", underwaterFogColor);
        Shader.SetGlobalFloat("_WaterSurfaceY", waterSurfaceY);
        Shader.SetGlobalFloat("_MaxDepth", maxDepth);
        Shader.SetGlobalFloat("_MaxFogDensity", maxFogDensity);
    }
}