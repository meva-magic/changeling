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
    
    [Header("Localization Keys")]
    public string interactionNameKey = "curtain";
    
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
        if (openModel != null) openModel.SetActive(true);
        if (closedModel != null) closedModel.SetActive(false);
        
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
        // If curtain is defeated and closed, don't do anything
        if (monsterDefeated) return;
        
        // Check if player is still holding the interaction key
        bool isStillHolding = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
        
        // If not holding and was holding before, reset everything
        if (!isStillHolding && isHolding)
        {
            ResetCurtainToOpen();
        }
    }
    
    public void Interact()
    {
        // Start holding when player interacts
        if (!isHolding && !monsterDefeated)
        {
            StartHolding();
        }
    }
    
    private void StartHolding()
    {
        if (isHolding || monsterDefeated) return;
        
        isHolding = true;
        
        // Check if monster exists
        bool hasMonster = targetWindow != null && targetWindow.HasActiveMonster;
        
        if (hasMonster)
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
        if (openModel != null) openModel.SetActive(false);
        if (closedModel != null) closedModel.SetActive(true);
    }
    
    private IEnumerator FillRoutine()
    {
        float fillTimer = 0f;
        
        while (isHolding && !monsterDefeated)
        {
            // Check if player is still holding
            bool isStillHolding = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
            
            if (!isStillHolding)
            {
                ResetCurtainToOpen();
                yield break;
            }
            
            // Check if still in range
            if (!IsInRange())
            {
                ResetCurtainToOpen();
                yield break;
            }
            
            // Check if monster still exists
            bool hasMonster = targetWindow != null && targetWindow.HasActiveMonster;
            if (!hasMonster)
            {
                // Monster disappeared, just reset to open
                ResetCurtainToOpen();
                yield break;
            }
            
            // Fill progress
            fillTimer += fillRate * Time.deltaTime;
            currentProgress = Mathf.Min(fillTimer, holdTimeRequired);
            UpdateProgressUI(currentProgress / holdTimeRequired);
            
            // Check if complete
            if (fillTimer >= holdTimeRequired)
            {
                DefeatMonster();
                yield break;
            }
            
            yield return null;
        }
    }
    
    private void ResetCurtainToOpen()
    {
        if (monsterDefeated) return;
        
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
        
        // ALWAYS open curtain when not holding
        if (openModel != null) openModel.SetActive(true);
        if (closedModel != null) closedModel.SetActive(false);
        
        // Play cancel sound if there was progress
        if (currentProgress > 0)
            PlaySound(cancelSoundName);
    }
    
    private void DefeatMonster()
    {
        monsterDefeated = true;
        isHolding = false;
        
        PlaySound(closeCompleteSoundName);
        
        if (targetWindow != null && targetWindow.HasActiveMonster)
            targetWindow.DespawnMonster();
        
        // Hide progress panel
        if (progressPanel != null)
            progressPanel.SetActive(false);
        
        // Reset progress UI
        UpdateProgressUI(0);
        
        // Keep curtain closed permanently (monster defeated)
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
        return GetLocalizedText(interactionNameKey);
    }
    
    private string GetLocalizedText(string key)
    {
        var table = UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase;
        if (table != null && !string.IsNullOrEmpty(key))
            return table.GetLocalizedString("UI Table", key);
        return key;
    }
    
    public void ResetCurtain()
    {
        StopAllCoroutines();
        isHolding = false;
        monsterDefeated = false;
        currentProgress = 0;
        
        if (openModel != null) openModel.SetActive(true);
        if (closedModel != null) closedModel.SetActive(false);
        if (progressPanel != null) progressPanel.SetActive(false);
        if (progressSlider != null) progressSlider.value = 0;
        if (fillImage != null) fillImage.fillAmount = 0;
    }
}