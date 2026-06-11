using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Candle : MonoBehaviour, IClickable
{
    [Header("Appearance")]
    [SerializeField] private Light candleLight;
    [SerializeField] private ParticleSystem candleFlame;
    
    [Header("Burn Settings")]
    [SerializeField] private float maxBurnTime = 60f;
    
    [Header("Timer UI")]
    [SerializeField] private GameObject timerCanvas;
    [SerializeField] private Image timerRadialImage;
    [SerializeField] private float displayRadius = 3f;
    
    [Header("Minigame")]
    [SerializeField] private float clickPower = 10f;
    [SerializeField] private float decaySpeed = 5f;
    [SerializeField] private float cooldownPeriod = 2f;
    [SerializeField] private float interactionRange = 2.5f;
    
    [Header("Feedback")]
    [SerializeField] private string extinguishedMessageKey = "candle_extinguished";
    [SerializeField] private float messageDuration = 3f;
    [SerializeField] private string relightSound = "candle_relight";
    
    private float remainingTime;
    private bool isLit = true;
    private bool isRelighting;
    private bool onCooldown;
    private Transform playerTransform;
    private bool messageShown;
    
    private void Start()
    {
        remainingTime = maxBurnTime;
        UpdateVisuals();
        UpdateTimerDisplay();
        
        if (timerCanvas != null) timerCanvas.SetActive(false);
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }
    
    private void Update()
    {
        if (isLit && !isRelighting)
        {
            remainingTime -= Time.deltaTime;
            UpdateTimerDisplay();
            
            if (remainingTime <= 0f)
            {
                Extinguish();
            }
            
            if (candleLight != null)
            {
                float percent = remainingTime / maxBurnTime;
                candleLight.intensity = Mathf.Lerp(0.3f, 1.5f, percent);
            }
        }
        
        UpdateTimerVisibility();
    }
    
    private void UpdateTimerVisibility()
    {
        if (timerCanvas == null || playerTransform == null) return;
        
        bool shouldShow = isLit && !isRelighting;
        
        if (shouldShow)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            shouldShow = distance <= displayRadius;
        }
        
        if (timerCanvas.activeSelf != shouldShow)
            timerCanvas.SetActive(shouldShow);
    }
    
    private void UpdateTimerDisplay()
    {
        if (timerRadialImage != null)
        {
            float percent = remainingTime / maxBurnTime;
            timerRadialImage.fillAmount = Mathf.Clamp01(percent);
        }
    }
    
    public void OnInteract()
    {
        if (!IsPlayerInRange()) return;
        if (isRelighting) return;
        if (onCooldown) return;
        
        BeginRelighting();
    }
    
    private bool IsPlayerInRange()
    {
        if (playerTransform == null) return true;
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= interactionRange;
    }
    
    private void BeginRelighting()
    {
        MinigameStarter minigame = ServiceLocator.Get<MinigameStarter>();
        if (minigame == null) return;
        
        isRelighting = true;
        messageShown = false;
        
        if (timerCanvas != null) timerCanvas.SetActive(false);
        
        MinigameConfiguration config = new MinigameConfiguration();
        config.Name = "Candle";
        config.ClickPower = clickPower;
        config.DecayRate = decaySpeed;
        config.UseMatchAnimation = true;
        config.SpawnFireOnFinish = true;
        config.LinkedObject = gameObject;
        config.OnFinished = OnRelightComplete;
        config.OnCancelled = OnRelightCancelled;
        
        minigame.StartMinigame(config);
    }
    
    private void OnRelightComplete()
    {
        isRelighting = false;
        isLit = true;
        remainingTime = maxBurnTime;
        UpdateTimerDisplay();
        UpdateVisuals();
        
        if (!string.IsNullOrEmpty(relightSound))
            AudioManager.instance?.Play(relightSound);
        
        StartCoroutine(CooldownRoutine());
        EventBus.Broadcast(GameEvents.CandleRelit);
    }
    
    private void OnRelightCancelled()
    {
        isRelighting = false;
    }
    
    private IEnumerator CooldownRoutine()
    {
        onCooldown = true;
        yield return new WaitForSeconds(cooldownPeriod);
        onCooldown = false;
    }
    
    private void Extinguish()
    {
        isLit = false;
        UpdateVisuals();
        
        if (!messageShown)
        {
            UserInterface ui = ServiceLocator.Get<UserInterface>();
            ui?.ShowMessage(extinguishedMessageKey, messageDuration);
            messageShown = true;
        }
        
        EventBus.Broadcast(GameEvents.CandleExtinguished);
    }
    
    private void UpdateVisuals()
    {
        if (candleLight != null) candleLight.enabled = isLit;
        
        if (candleFlame != null)
        {
            if (isLit && !candleFlame.isPlaying)
                candleFlame.Play();
            else if (!isLit && candleFlame.isPlaying)
                candleFlame.Stop();
        }
    }
    
    public string GetPromptKey()
    {
        return "";
    }
    
    public float GetInteractionRange()
    {
        return interactionRange;
    }
}