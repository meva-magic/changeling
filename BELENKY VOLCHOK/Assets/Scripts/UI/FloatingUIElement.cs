using UnityEngine;

public class FloatingUIElement : MonoBehaviour
{
    [SerializeField] private float floatHeight = 5f;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float rotationAngle = 3f;
    [SerializeField] private float rotationSpeed = 0.8f;
    [SerializeField] private AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool randomizePhase = true;
    
    private RectTransform rect;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float phaseOffset;
    
    private void Start()
    {
        rect = GetComponent<RectTransform>();
        if (rect == null)
        {
            enabled = false;
            return;
        }
        
        originalPosition = rect.anchoredPosition3D;
        originalRotation = rect.localRotation;
        
        if (randomizePhase)
            phaseOffset = Random.Range(0f, 360f);
    }
    
    private void Update()
    {
        float time = Time.time + phaseOffset;
        
        float sinValue = Mathf.Sin(time * floatSpeed);
        float normalized = (sinValue + 1f) / 2f;
        float eased = easingCurve.Evaluate(normalized);
        float finalOffset = floatHeight * (eased * 2f - 1f);
        
        Vector3 newPos = originalPosition;
        newPos.y = originalPosition.y + finalOffset;
        rect.anchoredPosition3D = newPos;
        
        if (enableRotation)
        {
            float rotValue = Mathf.Sin(time * rotationSpeed) * rotationAngle;
            rect.localRotation = originalRotation * Quaternion.Euler(0, 0, rotValue);
        }
    }
}
