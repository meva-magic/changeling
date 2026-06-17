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
    
    private float globalProgress = 0f;
    private bool isActive = false;
    private bool isPaused = false;
    private int activeCounterSources = 0;
    private float fillSpeed = 0f;
    private float savedProgress = 0f;
    private bool resumeWithSavedProgress = true;
    private float minOverlayAlpha = 0f;
    private bool isDeathTriggered = false;
    private Camera mainCamera;
    private Quaternion originalRotation;
    
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
        if (isActive && !isPaused && !isDeathTriggered)
        {
            globalProgress += fillSpeed * Time.deltaTime;
            
            if (globalProgress >= 100f)
            {
                globalProgress = 100f;
                isActive = false;
                activeCounterSources = 0;
                
                if (!isDeathTriggered)
                {
                    isDeathTriggered = true;
                    Debug.Log("[ThreatSystem] Угроза достигла 100%! Вызываем смерть");
                    TriggerDeath();
                }
            }
        }
        
        float fadeValue = globalProgress / 100f;
        UpdateUI(fadeValue);
        
        if (fadeValue > 0.01f && mainCamera != null)
        {
            float shakeAmount = shakeCurve.Evaluate(fadeValue) * maxShakeMagnitude;
            float shakeX = Random.Range(-shakeAmount, shakeAmount);
            float shakeY = Random.Range(-shakeAmount, shakeAmount);
            float shakeZ = Random.Range(-shakeAmount * 0.5f, shakeAmount * 0.5f);
            mainCamera.transform.localRotation = originalRotation * Quaternion.Euler(shakeX, shakeY, shakeZ);
        }
        else if (mainCamera != null && fadeValue <= 0.01f)
        {
            mainCamera.transform.localRotation = originalRotation;
        }
    }
    
    private void TriggerDeath()
    {
        // Сохраняем позицию игрока перед смертью
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Принудительно останавливаем физику
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                // Просто отключаем контроллер на время
            }
        }
        
        // Останавливаем камеру
        if (mainCamera != null)
        {
            mainCamera.transform.localRotation = originalRotation;
        }
        
        // Вызываем PenaltySystem
        PenaltySystem.Instance?.TriggerDeath();
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
    }
    
    public void AddCounterSource()
    {
        if (isDeathTriggered) return;
        activeCounterSources++;
        
        if (activeCounterSources > 0 && !isActive)
        {
            isActive = true;
            isPaused = false;
            globalProgress = 0f;
            savedProgress = 0f;
            isDeathTriggered = false;
        }
        else if (activeCounterSources > 0 && isPaused)
        {
            isPaused = false;
        }
    }
    
    public void RemoveCounterSource()
    {
        if (isDeathTriggered) return;
        activeCounterSources = Mathf.Max(0, activeCounterSources - 1);
        
        if (activeCounterSources <= 0)
        {
            isActive = false;
            isPaused = false;
        }
    }
    
    public void ResetAll()
    {
        activeCounterSources = 0;
        isActive = false;
        isPaused = false;
        globalProgress = 0f;
        savedProgress = 0f;
        isDeathTriggered = false;
        UpdateUI(0f);
        if (mainCamera != null)
            mainCamera.transform.localRotation = originalRotation;
    }
    
    public void StopCounter()
    {
        isActive = false;
        isPaused = false;
        globalProgress = 0f;
        savedProgress = 0f;
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
        if (isDeathTriggered) return;
        globalProgress = Mathf.Clamp(progress, 0f, 100f);
        UpdateUI(globalProgress / 100f);
        
        if (globalProgress >= 100f && !isDeathTriggered)
        {
            isDeathTriggered = true;
            isActive = false;
            activeCounterSources = 0;
            TriggerDeath();
        }
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