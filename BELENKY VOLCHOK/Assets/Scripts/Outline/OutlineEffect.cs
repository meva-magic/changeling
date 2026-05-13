using UnityEngine;

public class TightSpriteOutline : MonoBehaviour
{
    private Outline outline;
    
    void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }
        
        if (outline != null)
            outline.enabled = false;
    }
    
    public void EnableOutline(bool enable, Color color, float width)
    {
        if (outline == null)
        {
            outline = GetComponent<Outline>();
            if (outline == null)
            {
                outline = gameObject.AddComponent<Outline>();
            }
        }
        
        if (outline != null)
        {
            outline.OutlineColor = color;
            outline.OutlineWidth = width;
            outline.enabled = enable;
        }
    }
}