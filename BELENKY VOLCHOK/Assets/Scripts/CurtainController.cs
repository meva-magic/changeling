using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CurtainController : MonoBehaviour, IInteractable
{
    [Header("Models")]
    public GameObject openModel;
    public GameObject closedModel;
    
    [Header("Hold Settings")]
    public float holdTimeRequired = 3f;
    public float fillRate = 1f;
    public float interactionRange = 3f;
    
    [Header("UI")]
    public GameObject progressPanel;
    public Slider progressSlider;
    public Image fillImage;
    
    [Header("Target")]
    public WindowPoint targetWindow;
    
    [Header("Audio")]
    public string holdStartSoundName = "CurtainHoldStart";
    public string closeCompleteSoundName = "CurtainClose";
    public string cancelSoundName = "CurtainCancel";
    
    private float currentProgress;
    private bool isHolding;
    private Transform playerTransform;
    private bool monsterDefeated = false;
    private Coroutine fillCoroutine;
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        
        // Set initial visual state - OPEN
        SetCurtainOpen();
        
        // Setup progress UI
        if (progressPanel != null)
            progressPanel.SetActive(false);
        
        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = holdTimeRequired;
            progressSlider.value = 0;
        }
        
        if (fillImage != null)
            fillImage.fillAmount = 0;
        
        monsterDefeated = false;
        currentProgress = 0;
    }
    
    private void Update()
    {
        // Check if player is still holding the interaction key
        bool isStillHolding = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
        
        // If not holding and was holding before, open the curtain
        if (!isStillHolding && isHolding)
        {
            StopHoldingAndOpen();
        }
    }
    
    public void Interact()
    {
        // Start holding when player interacts (even if monster was defeated, but curtain opens on release)
        if (!isHolding)
        {
            StartHolding();
        }
    }
    
    private void StartHolding()
    {
        if (isHolding) return;
        
        isHolding = true;
        
        // Check if monster exists
        bool hasMonster = targetWindow != null && targetWindow.HasActiveMonster;
        
        if (hasMonster && !monsterDefeated)
        {
            // Show progress panel
            if (progressPanel != null)
                progressPanel.SetActive(true);
            
            PlaySound(holdStartSoundName);
            
            // Start fill coroutine
            if (fillCoroutine != null)
                StopCoroutine(fillCoroutine);
            fillCoroutine = StartCoroutine(FillRoutine());
        }
        
        // Close curtain visual immediately when holding
        SetCurtainClosed();
    }
    
    private IEnumerator FillRoutine()
    {
        float fillTimer = currentProgress;
        
        while (isHolding)
        {
            // Check if player is still holding
            bool isStillHolding = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
            
            if (!isStillHolding)
            {
                StopHoldingAndOpen();
                yield break;
            }
            
            // Check if still in range
            if (!IsInRange())
            {
                StopHoldingAndOpen();
                yield break;
            }
            
            // Check if monster still exists
            bool hasMonster = targetWindow != null && targetWindow.HasActiveMonster;
            if (!hasMonster)
            {
                // Monster disappeared, just open curtain
                StopHoldingAndOpen();
                yield break;
            }
            
            // Fill progress
            fillTimer += fillRate * Time.deltaTime;
            currentProgress = Mathf.Min(fillTimer, holdTimeRequired);
            UpdateProgressUI(currentProgress / holdTimeRequired);
            
            // Check if complete - defeat monster
            if (fillTimer >= holdTimeRequired)
            {
                DefeatMonster();
                // Keep curtain closed - don't open until release
                yield break;
            }
            
            yield return null;
        }
    }
    
    private void StopHoldingAndOpen()
    {
        isHolding = false;
        
        if (fillCoroutine != null)
            StopCoroutine(fillCoroutine);
        
        // Reset progress
        currentProgress = 0;
        
        // Hide progress panel
        if (progressPanel != null)
            progressPanel.SetActive(false);
        
        // Reset progress UI
        UpdateProgressUI(0);
        
        // Open curtain when releasing (regardless of monster state)
        SetCurtainOpen();
        
        // Play cancel sound if there was progress
        if (currentProgress > 0)
            PlaySound(cancelSoundName);
        
        // Reset monster defeated flag after releasing (monster will respawn later)
        monsterDefeated = false;
    }
    
    private void DefeatMonster()
    {
        monsterDefeated = true;
        
        PlaySound(closeCompleteSoundName);
        
        if (targetWindow != null && targetWindow.HasActiveMonster)
            targetWindow.DespawnMonster();
        
        // Hide progress panel
        if (progressPanel != null)
            progressPanel.SetActive(false);
        
        // Reset progress UI
        UpdateProgressUI(0);
        
        // Curtain stays closed after defeating monster
        // It will only open when player releases the button
    }
    
    private void SetCurtainOpen()
    {
        if (openModel != null) openModel.SetActive(true);
        if (closedModel != null) closedModel.SetActive(false);
    }
    
    private void SetCurtainClosed()
    {
        if (openModel != null) openModel.SetActive(false);
        if (closedModel != null) closedModel.SetActive(true);
    }
    
    private void UpdateProgressUI(float normalizedProgress)
    {
        if (progressSlider != null)
            progressSlider.value = normalizedProgress * holdTimeRequired;
        
        if (fillImage != null)
            fillImage.fillAmount = normalizedProgress;
    }
    
    private bool IsInRange()
    {
        if (playerTransform == null) return true;
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= interactionRange;
    }
    
    private void PlaySound(string soundName)
    {
        if (!string.IsNullOrEmpty(soundName) && AudioManager.instance != null)
            AudioManager.instance.Play(soundName);
    }
    
    public string GetInteractionName()
    {
        return ""; // No interaction name
    }
    
    public void ResetCurtain()
    {
        StopAllCoroutines();
        isHolding = false;
        monsterDefeated = false;
        currentProgress = 0;
        
        SetCurtainOpen();
        
        if (progressPanel != null)
            progressPanel.SetActive(false);
        if (progressSlider != null)
            progressSlider.value = 0;
        if (fillImage != null)
            fillImage.fillAmount = 0;
    }
}