using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class CurtainOutline : MonoBehaviour
{
    public Color outlineColor = Color.white;
    [Range(0f, 0.5f)]
    public float outlineWidth = 0.05f;
    
    private Material outlineMaterial;
    private Material originalMaterial;
    private Renderer objectRenderer;
    private bool isOutlined = false;
    
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
            originalMaterial = objectRenderer.material;
        
        Shader shader = Shader.Find("GUI/Text Shader");
        if (shader != null)
            outlineMaterial = new Material(shader);
    }
    
    public void EnableOutline()
    {
        if (objectRenderer != null && outlineMaterial != null && !isOutlined)
        {
            objectRenderer.material = outlineMaterial;
            outlineMaterial.SetColor("_Color", outlineColor);
            isOutlined = true;
        }
    }
    
    public void DisableOutline()
    {
        if (objectRenderer != null && originalMaterial != null && isOutlined)
        {
            objectRenderer.material = originalMaterial;
            isOutlined = false;
        }
    }
}