using UnityEngine;
using System.Collections;

public class ThreatSystem : MonoBehaviour
{
    public static ThreatSystem Instance { get; private set; }
    
    [Header("UI")]
    [SerializeField] private UnityEngine.UI.Image threatOverlay;
    [SerializeField] private AnimationCurve fadeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    
    [Header("Screen Shake")]
    [SerializeField] private float maxShakeMagnitude = 3f;
    [SerializeField] private AnimationCurve shakeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    
    [Header("Settings")]
    [SerializeField] private float threatDuration = 20f;
    
    private Coroutine activeThreat;
    private float currentIntensity = 0f;
    private float targetIntensity = 0f;
    private float transitionSpeed = 1f;
    private bool isShaking = false;
    private System.Action onMaxReached;
    private bool isMaxReached = false;
    private Camera mainCamera;
    private Quaternion originalRotation;
    
    private float globalProgress = 0f;
    private bool isActive = false;
    private bool isPaused = false;
    private System.Action onTimeout;
    private int activeCounterSources = 0;
    private float fillSpeed = 0f;
    private float savedProgress = 0f;
    private bool wasActive = false;
    private float curtainStartProgress = 0f;
    private bool resumeWithSavedProgress = true;
    private float minOverlayAlpha = 0f;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        mainCamera = Camera.main;
        if (mainCamera != null)
            originalRotation = mainCamera.transform.localRotation;
        
        if (threatOverlay != null)
        {
            Color color = threatOverlay.color;
            color.a = 0f;
            threatOverlay.color = color;
        }
    }
    
    private void Start()
    {
        if (threatDuration > 0)
            fillSpeed = 100f / threatDuration;
        else
            fillSpeed = 5f;
    }
    
    private void Update()
    {
        if (isActive && !isPaused)
        {
            globalProgress += fillSpeed * Time.deltaTime;
            
            if (globalProgress >= 100f)
            {
                globalProgress = 100f;
                isActive = false;
                activeCounterSources = 0;
                if (onTimeout != null)
                {
                    var callback = onTimeout;
                    onTimeout = null;
                    callback?.Invoke();
                }
            }
        }
        
        float fadeValue = globalProgress / 100f;
        currentIntensity = fadeValue;
        UpdateUI(fadeValue);
        isShaking = fadeValue > 0.01f;
        
        if (isShaking && mainCamera != null)
        {
            float shakeAmount = shakeCurve.Evaluate(fadeValue) * maxShakeMagnitude;
            float shakeX = Random.Range(-shakeAmount, shakeAmount);
            float shakeY = Random.Range(-shakeAmount, shakeAmount);
            float shakeZ = Random.Range(-shakeAmount * 0.5f, shakeAmount * 0.5f);
            mainCamera.transform.localRotation = originalRotation * Quaternion.Euler(shakeX, shakeY, shakeZ);
        }
        
        if (fadeValue >= 0.99f && !isMaxReached)
        {
            isMaxReached = true;
            if (onMaxReached != null)
                onMaxReached.Invoke();
        }
        
        wasActive = isActive;
    }
    
    private void UpdateUI(float fadeValue)
    {
        if (threatOverlay != null)
        {
            Color color = threatOverlay.color;
            float alpha = Mathf.Lerp(minOverlayAlpha, 1f, fadeValue);
            color.a = alpha;
            threatOverlay.color = color;
        }
    }
    
    public void SetMinOverlayAlpha(float minAlpha)
    {
        minOverlayAlpha = Mathf.Clamp01(minAlpha);
        Debug.Log($"ThreatSystem: Минимальная прозрачность установлена на {minOverlayAlpha}");
    }
    
    public void AddCounterSource()
    {
        activeCounterSources++;
        Debug.Log($"[ThreatSystem] Активных угроз: {activeCounterSources}");
        
        if (activeCounterSources > 0 && !isActive)
        {
            isActive = true;
            isPaused = false;
            globalProgress = 0f;
            savedProgress = 0f;
            wasActive = true;
        }
        else if (activeCounterSources > 0 && isPaused)
        {
            isPaused = false;
        }
    }
    
    public void RemoveCounterSource()
    {
        activeCounterSources = Mathf.Max(0, activeCounterSources - 1);
        Debug.Log($"[ThreatSystem] Активных угроз: {activeCounterSources}");
        
        if (activeCounterSources <= 0)
        {
            isActive = false;
            isPaused = false;
        }
    }
    
    public void ResetProgress()
    {
        globalProgress = 0f;
        savedProgress = 0f;
        curtainStartProgress = 0f;
        isMaxReached = false;
        UpdateUI(0f);
        
        if (activeCounterSources > 0)
        {
            isActive = true;
            isPaused = false;
        }
    }
    
    public void ResetAllCounters()
    {
        activeCounterSources = 0;
        isActive = false;
        isPaused = false;
        wasActive = false;
        globalProgress = 0f;
        savedProgress = 0f;
        curtainStartProgress = 0f;
        onTimeout = null;
        isMaxReached = false;
        resumeWithSavedProgress = true;
        UpdateUI(0f);
        
        if (mainCamera != null)
            mainCamera.transform.localRotation = originalRotation;
    }
    
    public void StopCounterPermanently()
    {
        Debug.Log($"[ThreatSystem] Счётчик выключен навсегда");
        activeCounterSources = 0;
        isActive = false;
        isPaused = false;
        wasActive = false;
        globalProgress = 0f;
        savedProgress = 0f;
        curtainStartProgress = 0f;
        onTimeout = null;
        isMaxReached = false;
        resumeWithSavedProgress = true;
        UpdateUI(0f);
        
        if (mainCamera != null)
            mainCamera.transform.localRotation = originalRotation;
    }
    
    public int GetActiveSourceCount()
    {
        return activeCounterSources;
    }
    
    public void StopCounter()
    {
        if (activeCounterSources > 0)
        {
            Debug.Log($"[ThreatSystem] StopCounter игнорирован — есть источники ({activeCounterSources})");
            return;
        }
        
        isActive = false;
        isPaused = false;
        wasActive = false;
        globalProgress = 0f;
        savedProgress = 0f;
        curtainStartProgress = 0f;
        onTimeout = null;
        isMaxReached = false;
        resumeWithSavedProgress = true;
        UpdateUI(0f);
    }
    
    public void PauseCounter()
    {
        if (!isActive) return;
        isPaused = true;
        savedProgress = globalProgress;
        resumeWithSavedProgress = true;
    }
    
    public void PauseCounterWithoutSave()
    {
        if (!isActive) return;
        isPaused = true;
        resumeWithSavedProgress = false;
    }
    
    public void ResumeCounter()
    {
        if (!isActive) return;
        isPaused = false;
        
        if (resumeWithSavedProgress)
        {
            globalProgress = savedProgress;
        }
        
        resumeWithSavedProgress = true;
    }
    
    public void SetProgress(float progress)
    {
        globalProgress = Mathf.Clamp(progress, 0f, 100f);
        UpdateUI(globalProgress / 100f);
        isShaking = globalProgress > 1f;
        
        if (globalProgress >= 100f && isActive)
        {
            isActive = false;
            activeCounterSources = 0;
            curtainStartProgress = 0f;
            if (onTimeout != null)
            {
                var callback = onTimeout;
                onTimeout = null;
                callback?.Invoke();
            }
        }
    }
    
    public void SetCurtainStartProgress(float progress)
    {
        curtainStartProgress = progress;
    }
    
    public float GetCurtainStartProgress()
    {
        return curtainStartProgress;
    }
    
    public float GetProgress()
    {
        return globalProgress;
    }
    
    public bool IsActive()
    {
        return isActive;
    }
    
    public void ResetCamera()
    {
        if (mainCamera != null)
            mainCamera.transform.localRotation = originalRotation;
    }
}