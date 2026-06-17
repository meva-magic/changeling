using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CurtainController : MonoBehaviour, IClickable
{
    [Header("Visuals")]
    [SerializeField] private GameObject openVisual;
    [SerializeField] private GameObject closedVisual;
    
    [Header("Target Object for Outline")]
    [SerializeField] private GameObject outlineTarget;
    
    [Header("Settings")]
    [SerializeField] private float requiredHoldTime = 3f;
    [SerializeField] private float fillSpeed = 1f;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private WindowMonsterPoint linkedWindow;
    [SerializeField] private GameObject progressPanel;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Image progressFill;
    [SerializeField] private string movementSound = "curtain_sound";
    
    private float holdProgress;
    private bool isHolding;
    private Transform playerTransform;
    private bool monsterDefeated;
    private Coroutine fillProcess;
    private bool isTimerActive;
    private Outline cachedOutline;
    private bool wasOutlineEnabled;
    private PlayerMovement playerMovement;
    private bool counterSourceAdded = false;
    private bool isPausedByCurtain = false;
    private float progressAtHoldStart = 0f;
    private float currentProgressAtRelease = 0f;
    private bool isMonsterDefeatedProcessed = false;
    
    private GameObject EffectiveOutlineTarget
    {
        get { return outlineTarget != null ? outlineTarget : gameObject; }
    }
    
    private void Start()
    {
        cachedOutline = EffectiveOutlineTarget.GetComponent<Outline>();
        if (cachedOutline != null)
        {
            wasOutlineEnabled = cachedOutline.enabled;
        }
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerMovement = player.GetComponent<PlayerMovement>();
        }
        SetOpenState();
        if (progressPanel != null) progressPanel.SetActive(false);
        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = requiredHoldTime;
            progressSlider.value = 0;
        }
        
        isMonsterDefeatedProcessed = false;
    }
    
    private void Update()
    {
        bool stillHolding = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
        if (!stillHolding && isHolding) CancelHold();
        
        if (isHolding && linkedWindow != null && linkedWindow.HasActiveMonster && !monsterDefeated)
        {
            float holdProgressNormalized = holdProgress / requiredHoldTime;
            float threatProgress = Mathf.Max(0f, progressAtHoldStart * (1f - holdProgressNormalized));
            ThreatSystem.Instance?.SetProgress(threatProgress);
            currentProgressAtRelease = threatProgress;
        }
    }
    
    public void OnInteract()
    {
        if (!IsPlayerInRange()) return;
        if (!isHolding) BeginHold();
    }
    
    private bool IsPlayerInRange()
    {
        if (playerTransform == null) return true;
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= interactionDistance;
    }
    
    public float GetInteractionRange() { return interactionDistance; }
    public string GetPromptKey() { return ""; }
    public GameObject GetOutlineTarget() { return EffectiveOutlineTarget; }
    
    private void BeginHold()
    {
        if (isHolding) return;
        isHolding = true;
        
        bool hasMonster = linkedWindow != null && linkedWindow.HasActiveMonster;
        
        if (!hasMonster || monsterDefeated)
        {
            SetClosedState();
            return;
        }
        
        if (ThreatSystem.Instance != null)
        {
            progressAtHoldStart = ThreatSystem.Instance.GetProgress();
            ThreatSystem.Instance.PauseCounterWithoutSave();
            isPausedByCurtain = true;
            currentProgressAtRelease = progressAtHoldStart;
        }
        
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false);
        
        PlaySound();
        
        if (cachedOutline != null)
        {
            wasOutlineEnabled = cachedOutline.enabled;
            cachedOutline.enabled = false;
        }
        if (progressPanel != null) progressPanel.SetActive(true);
        if (fillProcess != null) StopCoroutine(fillProcess);
        fillProcess = StartCoroutine(FillProcess());
        
        if (!counterSourceAdded)
        {
            counterSourceAdded = true;
            ThreatSystem.Instance?.AddCounterSource();
        }
        isTimerActive = true;
        SetClosedState();
    }
    
    private IEnumerator FillProcess()
    {
        float elapsed = holdProgress;
        
        while (isHolding)
        {
            bool stillHolding = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
            if (!stillHolding) { CancelHold(); yield break; }
            if (!IsPlayerInRange()) { CancelHold(); yield break; }
            bool hasMonster = linkedWindow != null && linkedWindow.HasActiveMonster;
            if (!hasMonster) 
            { 
                CancelHold(); 
                yield break; 
            }
            if (monsterDefeated) 
            { 
                CancelHold(); 
                yield break; 
            }
            elapsed += fillSpeed * Time.deltaTime;
            holdProgress = Mathf.Min(elapsed, requiredHoldTime);
            UpdateProgress(holdProgress / requiredHoldTime);
            
            if (elapsed >= requiredHoldTime)
            {
                DefeatMonster();
                yield break;
            }
            yield return null;
        }
    }
    
    private void CancelHold()
    {
        isHolding = false;
        
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);
        
        if (isPausedByCurtain && ThreatSystem.Instance != null)
        {
            ThreatSystem.Instance.SetProgress(currentProgressAtRelease);
            ThreatSystem.Instance.ResumeCounter();
            isPausedByCurtain = false;
        }
        
        if (fillProcess != null) StopCoroutine(fillProcess);
        holdProgress = 0;
        if (progressPanel != null) progressPanel.SetActive(false);
        UpdateProgress(0);
        SetOpenState();
        PlaySound();
        
        if (counterSourceAdded && !monsterDefeated)
        {
            counterSourceAdded = false;
            ThreatSystem.Instance?.RemoveCounterSource();
        }
        
        RestoreOutline();
        isTimerActive = false;
        isMonsterDefeatedProcessed = false;
    }
    
    private void DefeatMonster()
    {
        if (isMonsterDefeatedProcessed) return;
        isMonsterDefeatedProcessed = true;
        
        monsterDefeated = true;
        PlaySound();
        
        if (linkedWindow != null && linkedWindow.HasActiveMonster)
        {
            linkedWindow.BanishMonster();
        }
        
        if (progressPanel != null) progressPanel.SetActive(false);
        UpdateProgress(0);
        
        if (counterSourceAdded)
        {
            counterSourceAdded = false;
            ThreatSystem.Instance?.RemoveCounterSource();
        }
        
        isTimerActive = false;
        progressAtHoldStart = 0f;
        currentProgressAtRelease = 0f;
        
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);
    }
    
    private void RestoreOutline()
    {
        if (cachedOutline != null)
        {
            cachedOutline.enabled = wasOutlineEnabled;
        }
    }
    
    private void UpdateProgress(float normalized)
    {
        if (progressSlider != null) progressSlider.value = normalized * requiredHoldTime;
        if (progressFill != null) progressFill.fillAmount = normalized;
    }
    
    private void SetOpenState()
    {
        if (openVisual != null) openVisual.SetActive(true);
        if (closedVisual != null) closedVisual.SetActive(false);
    }
    
    private void SetClosedState()
    {
        if (openVisual != null) openVisual.SetActive(false);
        if (closedVisual != null) closedVisual.SetActive(true);
    }
    
    private void PlaySound()
    {
        if (!string.IsNullOrEmpty(movementSound)) AudioManager.instance?.Play(movementSound);
    }
}