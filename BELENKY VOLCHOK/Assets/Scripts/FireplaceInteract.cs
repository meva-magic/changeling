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
    public float decayRate = 5f;
    public float interactionRange = 3f;
    
    [Header("Audio")]
    public string lightSoundName = "FireLightSound";
    public string failSoundName = "FailSound";
    
    private bool isLit;
    private bool isLighting;
    private Transform playerTransform;
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }
    
    public void Interact()
    {
        if (!IsInRange())
        {
            Debug.Log("Too far from fireplace");
            return;
        }
        
        Debug.Log("Fireplace Interact called");
        
        if (isLit)
        {
            ShowMessage("fire_already_lit");
            if (!string.IsNullOrEmpty(failSoundName))
                PlaySound(failSoundName);
            return;
        }
        
        bool woodCollected = IsWoodCollected();
        Debug.Log($"Wood collected: {woodCollected}");
        
        if (!woodCollected)
        {
            ShowMessage("need_gather_wood");
            if (!string.IsNullOrEmpty(failSoundName))
                PlaySound(failSoundName);
            return;
        }
        
        StartLightingMinigame();
    }
    
    private bool IsInRange()
    {
        if (playerTransform == null) return true;
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= interactionRange;
    }
    
    private void StartLightingMinigame()
    {
        if (ClickerMinigameSystem.Instance == null)
        {
            Debug.LogWarning("ClickerMinigameSystem not found!");
            return;
        }
        
        if (isLighting) return;
        
        isLighting = true;
        
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
        
        ClickerMinigameSystem.Instance.StartMinigame(data);
    }
    
    private void OnLightingComplete()
    {
        isLighting = false;
        isLit = true;
        
        if (fireParticle != null)
        {
            fireParticle.Play();
        }
        
        if (firewoodModel != null)
        {
            firewoodModel.SetActive(true);
        }
        
        if (!string.IsNullOrEmpty(lightSoundName))
            PlaySound(lightSoundName);
        
        QuestManager.Instance?.TryCompleteQuestAction("Fireplace");
    }
    
    private void OnLightingCancel()
    {
        isLighting = false;
    }
    
    private bool IsWoodCollected()
    {
        if (QuestManager.Instance == null) return true;
        
        QuestStage stage = QuestManager.Instance.GetCurrentStage();
        
        if (stage == null) return true;
        
        if (string.Equals(stage.targetTag, "Fireplace", System.StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        if (string.Equals(stage.targetTag, "Firewood", System.StringComparison.OrdinalIgnoreCase) && stage.requiredAmount > 0)
        {
            return QuestManager.Instance.IsStageComplete();
        }
        
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