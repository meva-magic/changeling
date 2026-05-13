using UnityEngine;
using UnityEngine.UI;

public class CandleSystem : MonoBehaviour, IInteractable
{
    [Header("Candle Settings")]
    public string candleNameKey = "candle";
    public Light candleLight;
    public ParticleSystem fireParticle;
    public float maxBurnTime = 60f;
    
    [Header("Timer UI")]
    public GameObject timerPanel;
    public Image circleTimerImage;
    public float showTimerDistance = 3f;
    
    [Header("Minigame Settings")]
    public float weightPerClick = 10f;
    public float decayRate = 0.5f;
    public bool showMatchAnimation = true;
    public bool showFireEffect = true;
    
    [Header("Message Settings")]
    public string candleExtinguishedMessageKey = "candle_extinguished";
    public float messageDuration = 3f;
    
    [Header("Audio")]
    public string extinguishSoundName = "ExtinguishSound";
    public string relightSoundName = "RelightSound";
    
    private float currentBurnTime;
    private bool isLit = true;
    private bool isRelighting;
    private Camera mainCamera;
    private Transform playerTransform;
    private bool hasShownExtinguishMessage;
    
    private void Start()
    {
        currentBurnTime = maxBurnTime;
        UpdateCandleVisuals();
        
        if (timerPanel != null)
            timerPanel.SetActive(false);
        
        mainCamera = Camera.main;
        if (mainCamera != null)
            playerTransform = mainCamera.transform;
    }
    
    private void Update()
    {
        if (!isLit || isRelighting) return;
        
        currentBurnTime -= Time.deltaTime;
        
        if (circleTimerImage != null)
        {
            float percent = currentBurnTime / maxBurnTime;
            circleTimerImage.fillAmount = percent;
        }
        
        if (timerPanel != null && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool shouldShow = distance <= showTimerDistance && !isRelighting;
            
            if (timerPanel.activeSelf != shouldShow)
                timerPanel.SetActive(shouldShow);
        }
        
        if (currentBurnTime <= 0f)
        {
            ExtinguishCandle();
        }
        
        if (candleLight != null)
        {
            float percent = currentBurnTime / maxBurnTime;
            candleLight.intensity = Mathf.Lerp(0.3f, 1.5f, percent);
        }
    }
    
    public void Interact()
    {
        if (isRelighting) return;
        StartRelightMinigame();
    }
    
    private void StartRelightMinigame()
    {
        if (ClickerMinigameSystem.Instance == null) return;
        
        isRelighting = true;
        hasShownExtinguishMessage = false;
        
        if (timerPanel != null)
            timerPanel.SetActive(false);
        
        var data = new ClickerMinigameSystem.MinigameData
        {
            minigameId = "CandleRelight",
            weightPerClick = weightPerClick,
            decayRate = decayRate,
            showMatchAnimation = showMatchAnimation,
            showFireEffect = showFireEffect,
            targetObject = gameObject,
            onComplete = OnRelightComplete,
            onCancel = OnRelightCancel
        };
        
        ClickerMinigameSystem.Instance.StartMinigame(data);
    }
    
    private void OnRelightComplete()
    {
        isRelighting = false;
        isLit = true;
        currentBurnTime = maxBurnTime;
        UpdateCandleVisuals();
        
        if (!string.IsNullOrEmpty(relightSoundName))
            PlaySound(relightSoundName);
    }
    
    private void OnRelightCancel()
    {
        isRelighting = false;
        
        if (isLit && timerPanel != null && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            timerPanel.SetActive(distance <= showTimerDistance);
        }
    }
    
    private void ExtinguishCandle()
    {
        isLit = false;
        UpdateCandleVisuals();
        
        if (!string.IsNullOrEmpty(extinguishSoundName))
            PlaySound(extinguishSoundName);
        
        if (!hasShownExtinguishMessage && UIMessageManager.Instance != null)
        {
            UIMessageManager.Instance.ShowMessage(candleExtinguishedMessageKey, messageDuration);
            hasShownExtinguishMessage = true;
        }
    }
    
    private void UpdateCandleVisuals()
    {
        if (candleLight != null)
            candleLight.enabled = isLit;
        
        if (fireParticle != null)
        {
            if (isLit && !fireParticle.isPlaying)
                fireParticle.Play();
            else if (!isLit && fireParticle.isPlaying)
                fireParticle.Stop();
        }
    }
    
    private void PlaySound(string soundName)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.Play(soundName);
    }
    
    public string GetInteractionName()
    {
        string status = isLit ? " (Lit)" : " (Out)";
        return GetLocalizedText(candleNameKey) + status;
    }
    
    private string GetLocalizedText(string key)
    {
        var table = UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase;
        if (table != null) return table.GetLocalizedString("UI Table", key);
        return key;
    }
    
    private void OnDestroy()
    {
        if (timerPanel != null)
            timerPanel.SetActive(false);
    }
}