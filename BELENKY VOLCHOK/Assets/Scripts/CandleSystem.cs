using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Localization;

public class CandleSystem : MonoBehaviour, IInteractable
{
    [Header("Candle Settings")]
    public Light candleLight;
    public ParticleSystem fireParticle;
    public float maxBurnTime = 60f;
    
    [Header("Timer UI")]
    public GameObject timerPanel;
    public Image circleTimerImage;
    public float showTimerDistance = 3f;
    
    [Header("Minigame Settings")]
    public float weightPerClick = 10f;
    public float decayRate = 5f;
    public bool showMatchAnimation = true;
    public bool showFireEffect = true;
    public float minigameCooldown = 2f;
    
    [Header("Message Settings")]
    public string candleExtinguishedMessageKey = "candle_extinguished";
    public float messageDuration = 3f;
    
    [Header("Audio")]
    public string extinguishSoundName = "ExtinguishSound";
    public string relightSoundName = "RelightSound";
    
    [Header("Localization")]
    public LocalizedStringTable stringTable; // This gives you a dropdown!
    
    private float currentBurnTime;
    private bool isLit = true;
    private bool isRelighting;
    private bool isOnCooldown;
    private Camera mainCamera;
    private Transform playerTransform;
    private bool hasShownExtinguishMessage;
    
    private void Start()
    {
        currentBurnTime = maxBurnTime;
        UpdateCandleVisuals();
        UpdateTimerDisplay();
        
        if (timerPanel != null)
            timerPanel.SetActive(false);
        
        mainCamera = Camera.main;
        if (mainCamera != null)
            playerTransform = mainCamera.transform;
    }
    
    private void Update()
    {
        // Update burn time only if lit and not in minigame
        if (isLit && !isRelighting)
        {
            currentBurnTime -= Time.deltaTime;
            UpdateTimerDisplay();
            
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
        
        // Timer visibility - independent of cooldown, only affected by distance and minigame
        UpdateTimerVisibility();
    }
    
    private void UpdateTimerVisibility()
    {
        if (timerPanel == null || playerTransform == null) return;
        
        // Only show timer if candle is lit AND not in minigame
        bool shouldShow = isLit && !isRelighting;
        
        // Also check distance
        if (shouldShow)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            shouldShow = distance <= showTimerDistance;
        }
        
        if (timerPanel.activeSelf != shouldShow)
            timerPanel.SetActive(shouldShow);
    }
    
    private void UpdateTimerDisplay()
    {
        if (circleTimerImage != null)
        {
            float percent = currentBurnTime / maxBurnTime;
            circleTimerImage.fillAmount = Mathf.Clamp01(percent);
        }
    }
    
    public void Interact()
    {
        if (isRelighting)
        {
            Debug.Log("Already relighting");
            return;
        }
        
        if (isOnCooldown)
        {
            Debug.Log($"On cooldown, wait {minigameCooldown} seconds");
            return;
        }
        
        StartRelightMinigame();
    }
    
    private void StartRelightMinigame()
    {
        if (ClickerMinigameSystem.Instance == null)
        {
            Debug.LogWarning("ClickerMinigameSystem not found!");
            return;
        }
        
        isRelighting = true;
        hasShownExtinguishMessage = false;
        
        // Hide timer during minigame
        if (timerPanel != null)
            timerPanel.SetActive(false);
        
        Debug.Log($"Starting candle minigame with weightPerClick={weightPerClick}, decayRate={decayRate}");
        
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
        Debug.Log("Candle relight complete!");
        isRelighting = false;
        isLit = true;
        currentBurnTime = maxBurnTime;
        UpdateTimerDisplay();
        UpdateCandleVisuals();
        
        if (!string.IsNullOrEmpty(relightSoundName))
            PlaySound(relightSoundName);
        
        // Start cooldown
        StartCoroutine(CooldownRoutine());
    }
    
    private void OnRelightCancel()
    {
        Debug.Log("Candle relight cancelled");
        isRelighting = false;
    }
    
    private IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(minigameCooldown);
        isOnCooldown = false;
    }
    
    private void ExtinguishCandle()
    {
        isLit = false;
        UpdateCandleVisuals();
        
        if (!string.IsNullOrEmpty(extinguishSoundName))
            PlaySound(extinguishSoundName);
        
        if (!hasShownExtinguishMessage && UIMessageManager.Instance != null)
        {
            string localizedMessage = GetLocalizedText(candleExtinguishedMessageKey);
            UIMessageManager.Instance.ShowMessage(localizedMessage, messageDuration);
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
    
    private string GetLocalizedText(string key)
    {
        if (stringTable.IsEmpty)
        {
            Debug.LogWarning("String Table not assigned in CandleSystem! Please assign in inspector.");
            return key;
        }
        
        var table = stringTable.GetTable();
        if (table != null)
        {
            var entry = table.GetEntry(key);
            if (entry != null)
            {
                return entry.GetLocalizedString();
            }
        }
        
        Debug.LogWarning($"Key '{key}' not found in String Table");
        return key;
    }
    
    public string GetInteractionName()
    {
        return ""; // No interaction name
    }
    
    private void OnDestroy()
    {
        if (timerPanel != null)
            timerPanel.SetActive(false);
    }
}