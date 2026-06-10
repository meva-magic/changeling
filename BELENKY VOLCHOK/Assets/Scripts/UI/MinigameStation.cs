using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MinigameStation : MonoBehaviour, MinigameStarter
{
    [Header("UI References")]
    [SerializeField] private GameObject minigameCanvas;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image matchIcon;
    [SerializeField] private RectTransform matchStartPoint;
    [SerializeField] private RectTransform matchEndPoint;
    
    [Header("Animation")]
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem fireEffectPrefab;
    
    [Header("Sound Settings")]
    [SerializeField] private string clickSoundName = "minigame_click";
    [SerializeField] private string fireSoundName = "minigame_fire_spawn";
    
    private MinigameConfiguration activeConfig;
    private float currentValue;
    private bool isRunning;
    private bool waitingForEffect;
    private Vector3 savedPlayerPosition;
    private Quaternion savedPlayerRotation;
    
    private void Start()
    {
        if (minigameCanvas != null)
            minigameCanvas.SetActive(false);
    }
    
    private void Update()
    {
        if (!isRunning || waitingForEffect) return;
        
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            ProcessClick();
        }
        
        if (activeConfig != null && activeConfig.DecayRate > 0 && currentValue > 0)
        {
            currentValue -= activeConfig.DecayRate * Time.deltaTime;
            currentValue = Mathf.Max(currentValue, 0);
            UpdateDisplay(currentValue / activeConfig.TargetProgress);
            
            if (currentValue <= 0)
            {
                CancelCurrentMinigame();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelCurrentMinigame();
        }
    }
    
    public void StartMinigame(MinigameConfiguration config)
    {
        if (isRunning) return;
        
        activeConfig = config;
        currentValue = 0;
        isRunning = true;
        waitingForEffect = false;
        
        FreezePlayer();
        
        if (minigameCanvas != null)
            minigameCanvas.SetActive(true);
        
        UpdateDisplay(0);
        
        UserInterface ui = ServiceLocator.Get<UserInterface>();
        ui?.HideMessage();
        
        EventBus.Broadcast(GameEvents.MinigameStarted);
    }
    
    private void ProcessClick()
    {
        if (!isRunning || waitingForEffect) return;
        
        currentValue = Mathf.Min(currentValue + activeConfig.ClickPower, activeConfig.TargetProgress);
        UpdateDisplay(currentValue / activeConfig.TargetProgress);
        
        if (!string.IsNullOrEmpty(clickSoundName))
            AudioManager.instance?.Play(clickSoundName);
        
        if (currentValue >= activeConfig.TargetProgress)
        {
            FinishMinigame();
        }
    }
    
    private void UpdateDisplay(float normalizedValue)
    {
        if (progressBar != null)
            progressBar.value = normalizedValue;
        
        if (matchIcon != null && matchStartPoint != null && matchEndPoint != null)
        {
            Vector3 targetPosition = Vector3.Lerp(matchStartPoint.position, matchEndPoint.position, normalizedValue);
            matchIcon.rectTransform.position = targetPosition;
        }
    }
    
    private void FinishMinigame()
    {
        isRunning = false;
        
        if (activeConfig.SpawnFireOnFinish && fireEffectPrefab != null)
        {
            Vector3 spawnPosition = activeConfig.LinkedObject != null 
                ? activeConfig.LinkedObject.transform.position 
                : Vector3.zero;
            
            ParticleSystem fire = Instantiate(fireEffectPrefab, spawnPosition, Quaternion.identity);
            fire.Play();
            Destroy(fire.gameObject, 3f);
            
            if (!string.IsNullOrEmpty(fireSoundName))
                AudioManager.instance?.Play(fireSoundName);
            
            waitingForEffect = true;
            StartCoroutine(DelayedFinish(0.5f));
        }
        else
        {
            CloseMinigame();
            activeConfig.OnFinished?.Invoke();
            RestorePlayer();
        }
        
        EventBus.Broadcast(GameEvents.MinigameFinished);
    }
    
    private IEnumerator DelayedFinish(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseMinigame();
        activeConfig.OnFinished?.Invoke();
        RestorePlayer();
        waitingForEffect = false;
    }
    
    private void CloseMinigame()
    {
        if (minigameCanvas != null)
            minigameCanvas.SetActive(false);
        
        activeConfig = null;
        currentValue = 0;
        isRunning = false;
    }
    
    public void CancelCurrentMinigame()
    {
        if (!isRunning || waitingForEffect) return;
        
        isRunning = false;
        
        if (minigameCanvas != null)
            minigameCanvas.SetActive(false);
        
        activeConfig?.OnCancelled?.Invoke();
        activeConfig = null;
        currentValue = 0;
        
        RestorePlayer();
        EventBus.Broadcast(GameEvents.MinigameCancelled);
    }
    
    private void FreezePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            savedPlayerPosition = player.transform.position;
            savedPlayerRotation = player.transform.rotation;
            
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            
            MouseLook mouseLook = player.GetComponent<MouseLook>();
            if (mouseLook != null) mouseLook.enabled = false;
            
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null) movement.enabled = false;
        }
        
        CursorController cursor = ServiceLocator.Get<CursorController>();
        cursor?.UnlockForUI();
    }
    
    private void RestorePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = savedPlayerPosition;
            player.transform.rotation = savedPlayerRotation;
            
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;
            
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = true;
            
            MouseLook mouseLook = player.GetComponent<MouseLook>();
            if (mouseLook != null) mouseLook.enabled = true;
            
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null) movement.enabled = true;
        }
        
        CursorController cursor = ServiceLocator.Get<CursorController>();
        cursor?.LockForGameplay();
    }
    
    public bool IsMinigameActive => isRunning;
}

public class MinigameConfiguration
{
    public string Name { get; set; }
    public float TargetProgress { get; set; } = 100f;
    public float ClickPower { get; set; } = 10f;
    public float DecayRate { get; set; } = 5f;
    public bool UseMatchAnimation { get; set; } = true;
    public bool SpawnFireOnFinish { get; set; } = true;
    public System.Action OnFinished { get; set; }
    public System.Action OnCancelled { get; set; }
    public GameObject LinkedObject { get; set; }
}