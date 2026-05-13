using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Localization;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    
    [Header("Quest Settings")]
    public QuestData currentQuest;
    
    [Header("UI References")]
    public GameObject questWindow;
    public TextMeshProUGUI questObjectiveText;
    public TextMeshProUGUI questProgressText;
    
    [Header("Localization")]
    public LocalizedStringTable stringTable; // This gives you a dropdown!
    
    private int currentStageIndex;
    private int currentProgress;
    private Dictionary<string, int> collectedItems;
    private bool wasQuestWindowVisible;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        collectedItems = new Dictionary<string, int>();
    }
    
    private void Start()
    {
        if (questWindow != null)
            questWindow.SetActive(true);
        
        StartQuest();
    }
    
    private void Update()
    {
        // Check if minigame is active
        bool isMinigameActive = ClickerMinigameSystem.Instance != null && ClickerMinigameSystem.Instance.IsMinigameActive;
        
        if (isMinigameActive)
        {
            // Hide quest window during minigame if it was visible
            if (questWindow != null && questWindow.activeSelf)
            {
                wasQuestWindowVisible = true;
                questWindow.SetActive(false);
            }
        }
        else
        {
            // Show quest window after minigame if quest is not complete
            if (questWindow != null && wasQuestWindowVisible && !IsQuestComplete())
            {
                questWindow.SetActive(true);
                wasQuestWindowVisible = false;
            }
        }
    }
    
    public void StartQuest()
    {
        if (currentQuest == null)
        {
            Debug.LogWarning("No quest assigned to QuestManager!");
            return;
        }
        
        currentStageIndex = 0;
        currentProgress = 0;
        collectedItems.Clear();
        UpdateQuestUI();
        
        // Make sure quest window is visible
        if (questWindow != null)
            questWindow.SetActive(true);
        
        wasQuestWindowVisible = true;
        
        Debug.Log($"Quest started: Stage {currentStageIndex}, Required: {GetCurrentStage()?.requiredAmount}, TargetTag: '{GetCurrentStage()?.targetTag}'");
    }
    
    public void CollectItem(string itemTag, int amount = 1)
    {
        QuestStage stage = GetCurrentStage();
        if (stage == null)
        {
            Debug.Log("No current stage found");
            return;
        }
        
        Debug.Log($"CollectItem called: tag={itemTag}, stage.targetTag='{stage.targetTag}', currentProgress={currentProgress}, required={stage.requiredAmount}");
        
        if (stage.targetTag == itemTag && currentProgress < stage.requiredAmount)
        {
            if (!collectedItems.ContainsKey(itemTag))
                collectedItems[itemTag] = 0;
            
            collectedItems[itemTag] += amount;
            currentProgress = collectedItems[itemTag];
            
            UpdateQuestUI();
            Debug.Log($"Collected {currentProgress}/{stage.requiredAmount}");
            
            if (currentProgress >= stage.requiredAmount)
            {
                Debug.Log("Stage complete! Moving to next stage");
                CompleteCurrentStage();
            }
        }
        else
        {
            Debug.Log($"Cannot collect: stage.targetTag='{stage.targetTag}', itemTag={itemTag}, currentProgress={currentProgress}, required={stage.requiredAmount}");
        }
    }
    
    public bool CanInteractWith(string objectTag)
    {
        QuestStage stage = GetCurrentStage();
        if (stage == null) return false;
        
        Debug.Log($"CanInteractWith: objectTag={objectTag}, stage.targetTag='{stage.targetTag}', required={stage.requiredAmount}");
        
        if (stage.requiredAmount > 0 && stage.targetTag != objectTag)
        {
            ShowMessage("need_gather_wood");
            return false;
        }
        
        return true;
    }
    
    public void TryCompleteQuestAction(string objectTag)
    {
        QuestStage stage = GetCurrentStage();
        if (stage == null) return;
        
        Debug.Log($"TryCompleteQuestAction: objectTag={objectTag}, stage.targetTag='{stage.targetTag}', required={stage.requiredAmount}");
        
        if (stage.targetTag == objectTag && stage.requiredAmount == 0)
        {
            Debug.Log("Action stage complete!");
            CompleteCurrentStage();
        }
    }
    
    private void CompleteCurrentStage()
    {
        currentStageIndex++;
        
        if (currentStageIndex >= currentQuest.stages.Length)
        {
            CompleteQuest();
        }
        else
        {
            currentProgress = 0;
            collectedItems.Clear();
            UpdateQuestUI();
            Debug.Log($"Moved to stage {currentStageIndex}, Required: {GetCurrentStage()?.requiredAmount}, TargetTag: '{GetCurrentStage()?.targetTag}'");
        }
    }
    
    private void CompleteQuest()
    {
        Debug.Log("Quest completed!");
        
        // Close quest window when quest is complete
        if (questWindow != null)
        {
            questWindow.SetActive(false);
            wasQuestWindowVisible = false;
            Debug.Log("Quest window closed - quest complete");
        }
        
        if (questProgressText != null)
            questProgressText.text = "COMPLETE!";
    }
    
    public QuestStage GetCurrentStage()
    {
        if (currentQuest == null)
        {
            Debug.Log("currentQuest is null");
            return null;
        }
        
        if (currentStageIndex >= currentQuest.stages.Length)
        {
            Debug.Log("currentStageIndex is out of range");
            return null;
        }
        
        QuestStage stage = currentQuest.stages[currentStageIndex];
        return stage;
    }
    
    public bool IsStageComplete()
    {
        QuestStage stage = GetCurrentStage();
        if (stage == null) return false;
        
        if (stage.requiredAmount > 0)
            return currentProgress >= stage.requiredAmount;
        
        return false;
    }
    
    public bool IsQuestComplete()
    {
        if (currentQuest == null) return false;
        return currentStageIndex >= currentQuest.stages.Length;
    }
    
    private void ShowMessage(string key)
    {
        if (UIMessageManager.Instance != null)
            UIMessageManager.Instance.ShowMessage(key);
    }
    
    private void UpdateQuestUI()
    {
        if (currentQuest == null) return;
        
        QuestStage stage = GetCurrentStage();
        if (stage != null)
        {
            if (questObjectiveText != null)
                questObjectiveText.text = GetLocalizedText(stage.objectiveKey);
            
            if (questProgressText != null && stage.requiredAmount > 0)
                questProgressText.text = $"{currentProgress}/{stage.requiredAmount}";
            else if (questProgressText != null)
                questProgressText.text = "";
        }
    }
    
    private string GetLocalizedText(string key)
    {
        if (stringTable.IsEmpty)
        {
            Debug.LogWarning("String Table not assigned in QuestManager! Please assign in inspector.");
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
}