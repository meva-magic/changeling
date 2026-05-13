using UnityEngine;

public class FireplaceInteract : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public string interactionNameKey = "fireplace";
    public ParticleSystem fireParticle;
    public GameObject firewoodModel;
    public float weightPerClick = 10f;
    public bool showMatchAnimation = true;
    public bool showFireEffect = true;
    public float decayRate = 0.5f;
    
    [Header("Audio")]
    public string lightSoundName = "FireLightSound";
    public string failSoundName = "FailSound";
    
    private bool isLit;
    private bool isLighting;
    
    public void Interact()
    {
        Debug.Log("Fireplace Interact called");
        
        if (isLit)
        {
            Debug.Log("Fireplace already lit");
            ShowMessage("fire_already_lit");
            if (!string.IsNullOrEmpty(failSoundName))
                PlaySound(failSoundName);
            return;
        }
        
        // Check if wood collection stage is complete
        bool woodCollected = IsWoodCollected();
        Debug.Log($"Wood collected: {woodCollected}");
        
        if (!woodCollected)
        {
            Debug.Log("Wood not collected, showing message");
            ShowMessage("need_gather_wood");
            if (!string.IsNullOrEmpty(failSoundName))
                PlaySound(failSoundName);
            return;
        }
        
        Debug.Log("Starting lighting minigame...");
        StartLightingMinigame();
    }
    
    private void StartLightingMinigame()
    {
        Debug.Log("StartLightingMinigame called");
        
        if (ClickerMinigameSystem.Instance == null)
        {
            Debug.LogError("ClickerMinigameSystem.Instance is NULL! Make sure ClickerMinigameSystem is in the scene.");
            return;
        }
        
        Debug.Log($"ClickerMinigameSystem.Instance found: {ClickerMinigameSystem.Instance.gameObject.name}");
        
        if (isLighting)
        {
            Debug.Log("Already lighting, skipping");
            return;
        }
        
        isLighting = true;
        Debug.Log($"Creating minigame data with weightPerClick={weightPerClick}, decayRate={decayRate}");
        
        var data = new ClickerMinigameSystem.MinigameData
        {
            minigameId = "FireplaceLight",
            weightPerClick = weightPerClick,
            showMatchAnimation = showMatchAnimation,
            showFireEffect = showFireEffect,
            targetObject = gameObject,
            decayRate = decayRate,
            onComplete = OnLightingComplete,
            onCancel = OnLightingCancel
        };
        
        Debug.Log("Calling ClickerMinigameSystem.StartMinigame");
        ClickerMinigameSystem.Instance.StartMinigame(data);
    }
    
    private void OnLightingComplete()
    {
        Debug.Log("OnLightingComplete called");
        isLighting = false;
        isLit = true;
        
        if (fireParticle != null)
        {
            Debug.Log("Playing fire particle");
            fireParticle.Play();
        }
        else
        {
            Debug.Log("fireParticle is null");
        }
        
        if (firewoodModel != null)
        {
            Debug.Log("Activating firewood model");
            firewoodModel.SetActive(true);
        }
        else
        {
            Debug.Log("firewoodModel is null");
        }
        
        if (!string.IsNullOrEmpty(lightSoundName))
            PlaySound(lightSoundName);
        
        Debug.Log("Calling TryCompleteQuestAction");
        QuestManager.Instance?.TryCompleteQuestAction("Fireplace");
    }
    
    private void OnLightingCancel()
    {
        Debug.Log("OnLightingCancel called");
        isLighting = false;
    }
    
    private bool IsWoodCollected()
    {
        if (QuestManager.Instance == null)
        {
            Debug.Log("QuestManager.Instance is null");
            return true;
        }
        
        QuestStage stage = QuestManager.Instance.GetCurrentStage();
        
        if (stage == null)
        {
            Debug.Log("Current stage is null");
            return true;
        }
        
        Debug.Log($"Current stage - TargetTag: '{stage.targetTag}', RequiredAmount: {stage.requiredAmount}");
        
        if (string.Equals(stage.targetTag, "Fireplace", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("Current stage is Fireplace - wood collection is complete, can light fire!");
            return true;
        }
        
        if (string.Equals(stage.targetTag, "Firewood", System.StringComparison.OrdinalIgnoreCase) && stage.requiredAmount > 0)
        {
            bool isComplete = QuestManager.Instance.IsStageComplete();
            Debug.Log($"Still in wood collection stage. IsComplete: {isComplete}");
            return isComplete;
        }
        
        Debug.Log("Unknown stage state - returning false");
        return false;
    }
    
    private void ShowMessage(string key)
    {
        if (UIMessageManager.Instance != null)
            UIMessageManager.Instance.ShowMessage(key);
    }
    
    private void PlaySound(string soundName)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.Play(soundName);
    }
    
    public string GetInteractionName()
    {
        return GetLocalizedText(interactionNameKey);
    }
    
    private string GetLocalizedText(string key)
    {
        var table = UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase;
        if (table != null) return table.GetLocalizedString("UI Table", key);
        return key;
    }
}