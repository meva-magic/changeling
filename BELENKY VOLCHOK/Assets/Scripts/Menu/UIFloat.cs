using UnityEngine;

public class UIFloatingAnimation : MonoBehaviour
{
    [Header("Movement Settings")]
    public float floatAmplitude = 5f;      
    public float floatSpeed = 1f;           
    
    [Header("Rotation Settings")]
    public bool enableRotation = true;
    public float rotationAmplitude = 3f;   
    public float rotationSpeed = 0.8f;      
    
    [Header("Scale Settings")]
    public bool enableScalePulse = false;
    public float scaleAmplitude = 0.05f;   
    public float scaleSpeed = 1.2f;         
    
    [Header("Smoothing")]
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool useUnscaledTime = false;   
    
    [Header("Random Offset")]
    public bool randomizeStartOffset = true;
    public float randomOffsetRange = 360f;   
    
    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private float timeOffset;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogWarning("UIFloatingAnimation requires a RectTransform component!");
            enabled = false;
            return;
        }
        
        originalPosition = rectTransform.anchoredPosition3D;
        originalRotation = rectTransform.localRotation;
        originalScale = rectTransform.localScale;
        
        if (randomizeStartOffset)
        {
            timeOffset = Random.Range(0f, randomOffsetRange);
        }
        else
        {
            timeOffset = 0f;
        }
    }
    
    void Update()
    {
        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float time = (useUnscaledTime ? Time.unscaledTime : Time.time) + timeOffset;
        
        float floatValue = Mathf.Sin(time * floatSpeed) * floatAmplitude;
        float normalizedFloat = (Mathf.Sin(time * floatSpeed) + 1f) / 2f;
        float curvedValue = movementCurve.Evaluate(normalizedFloat);
        float finalOffset = floatAmplitude * (curvedValue * 2f - 1f);
        
        Vector3 newPosition = originalPosition;
        newPosition.y = originalPosition.y + finalOffset;
        rectTransform.anchoredPosition3D = newPosition;
        
        if (enableRotation)
        {
            float rotationValue = Mathf.Sin(time * rotationSpeed) * rotationAmplitude;
            Quaternion newRotation = originalRotation * Quaternion.Euler(0, 0, rotationValue);
            rectTransform.localRotation = newRotation;
        }
        
        if (enableScalePulse)
        {
            float scaleValue = 1f + Mathf.Sin(time * scaleSpeed) * scaleAmplitude;
            rectTransform.localScale = originalScale * scaleValue;
        }
    }
    
    public void ResetToOriginal()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition3D = originalPosition;
            rectTransform.localRotation = originalRotation;
            rectTransform.localScale = originalScale;
        }
    }
    
    public void SetFloatAmplitude(float newAmplitude)
    {
        floatAmplitude = newAmplitude;
    }
    
    public void SetFloatSpeed(float newSpeed)
    {
        floatSpeed = newSpeed;
    }
    
    public void SetPaused(bool paused)
    {
        enabled = !paused;
    }
}
