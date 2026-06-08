using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }
    
    [Header("Shake Settings")]
    public float defaultShakeDuration = 0.3f;
    public float defaultShakeMagnitude = 5f; // Degrees of rotation
    
    [Header("Decay Settings")]
    public AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public bool smoothShake = true;
    
    [Header("Camera Reference")]
    public Camera targetCamera; // Leave empty to use Camera.main
    
    private Quaternion originalRotation;
    private Coroutine shakeCoroutine;
    private bool isShaking = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        // Get camera reference if not assigned
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }
        }
        
        // Store original rotation
        if (targetCamera != null)
        {
            originalRotation = targetCamera.transform.localRotation;
        }
    }
    
    /// <summary>
    /// Shake the camera with default settings
    /// </summary>
    public void Shake()
    {
        Shake(defaultShakeDuration, defaultShakeMagnitude);
    }
    
    /// <summary>
    /// Shake the camera with custom duration and magnitude
    /// </summary>
    /// <param name="duration">How long the shake lasts in seconds</param>
    /// <param name="magnitude">Maximum rotation in degrees (Z-axis)</param>
    public void Shake(float duration, float magnitude)
    {
        if (targetCamera == null)
        {
            Debug.LogWarning("ScreenShake: No camera assigned!");
            return;
        }
        
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            // Reset camera rotation before starting new shake
            targetCamera.transform.localRotation = originalRotation;
        }
        
        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }
    
    /// <summary>
    /// Shake with intensity that fades over time
    /// </summary>
    /// <param name="duration">How long the shake lasts</param>
    /// <param name="startMagnitude">Starting rotation magnitude</param>
    /// <param name="endMagnitude">Ending rotation magnitude</param>
    public void ShakeFade(float duration, float startMagnitude, float endMagnitude)
    {
        if (targetCamera == null)
        {
            Debug.LogWarning("ScreenShake: No camera assigned!");
            return;
        }
        
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            targetCamera.transform.localRotation = originalRotation;
        }
        
        shakeCoroutine = StartCoroutine(ShakeFadeCoroutine(duration, startMagnitude, endMagnitude));
    }
    
    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            
            // Calculate current magnitude based on curve
            float curveValue = shakeCurve.Evaluate(normalizedTime);
            float currentMagnitude = magnitude * curveValue;
            
            // Generate random Z rotation (-1 to 1 range)
            float shakeX = Random.Range(-currentMagnitude, currentMagnitude);
            float shakeY = Random.Range(-currentMagnitude, currentMagnitude);
            float shakeZ = Random.Range(-currentMagnitude, currentMagnitude);
            
            // Apply shake rotation
            targetCamera.transform.localRotation = originalRotation * Quaternion.Euler(shakeX, shakeY, shakeZ);
            
            yield return null;
        }
        
        // Reset camera to original rotation
        targetCamera.transform.localRotation = originalRotation;
        isShaking = false;
        shakeCoroutine = null;
    }
    
    private IEnumerator ShakeFadeCoroutine(float duration, float startMagnitude, float endMagnitude)
    {
        isShaking = true;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            
            // Lerp between start and end magnitude
            float currentMagnitude = Mathf.Lerp(startMagnitude, endMagnitude, normalizedTime);
            
            // Apply curve for smoother feel
            float curveValue = shakeCurve.Evaluate(normalizedTime);
            currentMagnitude *= curveValue;
            
            // Generate random Z rotation
            float shakeX = Random.Range(-currentMagnitude, currentMagnitude);
            float shakeY = Random.Range(-currentMagnitude, currentMagnitude);
            float shakeZ = Random.Range(-currentMagnitude, currentMagnitude);
            
            // Apply shake rotation
            targetCamera.transform.localRotation = originalRotation * Quaternion.Euler(shakeX, shakeY, shakeZ);
            
            yield return null;
        }
        
        // Reset camera
        targetCamera.transform.localRotation = originalRotation;
        isShaking = false;
        shakeCoroutine = null;
    }
    
    /// <summary>
    /// Stop current shake immediately and reset camera
    /// </summary>
    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
        
        if (targetCamera != null)
        {
            targetCamera.transform.localRotation = originalRotation;
        }
        
        isShaking = false;
    }
    
    /// <summary>
    /// Check if camera is currently shaking
    /// </summary>
    public bool IsShaking()
    {
        return isShaking;
    }
    
    /// <summary>
    /// Update stored original rotation (call if camera rotation changes)
    /// </summary>
    public void UpdateOriginalRotation()
    {
        if (targetCamera != null)
        {
            originalRotation = targetCamera.transform.localRotation;
        }
    }
}
