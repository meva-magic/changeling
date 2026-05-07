using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Fog : MonoBehaviour
{
    [Header("Fog")]
    public Shader fogShader;
    public Color fogColor = Color.gray;
    
    [Range(0.0f, 1.0f)]
    public float fogDensity = 0.5f;
    
    [Range(0.0f, 100.0f)]
    public float fogOffset = 0.0f;
    
    private Material fogMaterial;

    void Start()
    {
        if (fogMaterial == null)
        {
            fogMaterial = new Material(fogShader);
            fogMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        Camera cam = GetComponent<Camera>();
        cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.Depth;
    }

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        fogMaterial.SetVector("_FogColor", fogColor);
        fogMaterial.SetFloat("_FogDensity", fogDensity);
        fogMaterial.SetFloat("_FogOffset", fogOffset);
        Graphics.Blit(source, destination, fogMaterial);
    }

    void OnDisable()
    {
        if (fogMaterial != null)
        {
            DestroyImmediate(fogMaterial);
            fogMaterial = null;
        }
    }
}