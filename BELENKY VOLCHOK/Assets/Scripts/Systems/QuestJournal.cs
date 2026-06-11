using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class QuestJournal : MonoBehaviour, QuestTracker
{
    [SerializeField] private GameObject journalWindow;
    [SerializeField] private TextMeshProUGUI objectiveField;
    [SerializeField] private TextMeshProUGUI progressField;
    
    private QuestData currentQuest;
    private int activeStage;
    private int stageProgress;
    private Dictionary<string, int> collectedItems;
    private bool wasVisible;
    private bool isQuestActive = false;
    
    private void Awake()
    {
        collectedItems = new Dictionary<string, int>();
    }
    
    private void Start()
    {
        if (journalWindow != null)
            journalWindow.SetActive(false);
    }
    
    private void OnEnable()
    {
        EventBus.Listen(GameEvents.MinigameStarted, OnMinigameStarted);
        EventBus.Listen(GameEvents.MinigameFinished, OnMinigameEnded);
        EventBus.Listen(GameEvents.MinigameCancelled, OnMinigameEnded);
    }
    
    private void OnDisable()
    {
        EventBus.StopListening(GameEvents.MinigameStarted, OnMinigameStarted);
        EventBus.StopListening(GameEvents.MinigameFinished, OnMinigameEnded);
        EventBus.StopListening(GameEvents.MinigameCancelled, OnMinigameEnded);
    }
    
    private void OnMinigameStarted()
    {
        if (journalWindow != null && journalWindow.activeSelf)
        {
            wasVisible = true;
            journalWindow.SetActive(false);
        }
    }
    
    private void OnMinigameEnded()
    {
        if (journalWindow != null && wasVisible && !IsQuestFinished() && isQuestActive)
        {
            journalWindow.SetActive(true);
            wasVisible = false;
        }
    }
    
    public void BeginQuest(QuestData quest)
    {
        currentQuest = quest;
        activeStage = 0;
        stageProgress = 0;
        collectedItems.Clear();
        isQuestActive = true;
        RefreshUI();
        
        if (journalWindow != null)
            journalWindow.SetActive(true);
        
        wasVisible = true;
        EventBus.Broadcast(GameEvents.QuestStarted);
        Debug.Log($"QuestJournal: Квест начат, стадия 0, RequiredTag={GetCurrentStage()?.RequiredTag}");
    }
    
    public void RecordCollectedItem(string itemTag)
    {
        if (!isQuestActive) return;
        
        QuestStageDefinition stage = GetCurrentStage();
        if (stage == null)
        {
            Debug.Log("QuestJournal: Нет активной стадии");
            return;
        }
        
        Debug.Log($"QuestJournal: RecordCollectedItem, stage.RequiredTag={stage.RequiredTag}, itemTag={itemTag}, stageProgress={stageProgress}, required={stage.RequiredQuantity}");
        
        if (stage.RequiredTag == itemTag && stageProgress < stage.RequiredQuantity)
        {
            if (!collectedItems.ContainsKey(itemTag))
                collectedItems[itemTag] = 0;
            
            collectedItems[itemTag]++;
            stageProgress = collectedItems[itemTag];
            
            RefreshUI();
            EventBus.Broadcast(GameEvents.FirewoodCollected);
            
            if (stageProgress >= stage.RequiredQuantity)
            {
                Debug.Log("QuestJournal: Сбор завершён, переходим на следующую стадию");
                AdvanceToNextStage();
            }
        }
    }
    
    public void CompleteObjective(string objectiveTag)
    {
        if (!isQuestActive) return;
        
        QuestStageDefinition stage = GetCurrentStage();
        if (stage == null)
        {
            Debug.Log("QuestJournal: CompleteObjective - нет активной стадии");
            return;
        }
        
        Debug.Log($"QuestJournal: CompleteObjective вызван, stage.RequiredTag={stage.RequiredTag}, objectiveTag={objectiveTag}, stage.RequiredQuantity={stage.RequiredQuantity}");
        
        if (stage.RequiredTag == objectiveTag && stage.RequiredQuantity == 0)
        {
            Debug.Log("QuestJournal: CompleteObjective - переходим на следующую стадию");
            AdvanceToNextStage();
        }
        else
        {
            Debug.Log("QuestJournal: CompleteObjective - условия не совпадают");
        }
    }
    
    private void AdvanceToNextStage()
    {
        activeStage++;
        
        if (activeStage >= currentQuest.Stages.Length)
        {
            FinishQuest();
        }
        else
        {
            stageProgress = 0;
            collectedItems.Clear();
            RefreshUI();
            EventBus.Broadcast(GameEvents.QuestStageAdvanced);
            Debug.Log($"QuestJournal: Переход на стадию {activeStage}, RequiredTag={GetCurrentStage()?.RequiredTag}");
        }
    }
    
    private void FinishQuest()
    {
        isQuestActive = false;
        
        if (journalWindow != null)
        {
            journalWindow.SetActive(false);
            wasVisible = false;
        }
        
        Debug.Log("QuestJournal: Квест завершён, отправляем событие QuestFinished");
        EventBus.Broadcast(GameEvents.QuestFinished);
    }
    
    public QuestStageDefinition GetCurrentStage()
    {
        if (!isQuestActive || currentQuest == null || activeStage >= currentQuest.Stages.Length)
            return null;
        return currentQuest.Stages[activeStage];
    }
    
    public bool IsCurrentStageFinished()
    {
        QuestStageDefinition stage = GetCurrentStage();
        if (stage == null) return false;
        
        if (stage.RequiredQuantity > 0)
            return stageProgress >= stage.RequiredQuantity;
        return false;
    }
    
    public bool IsQuestFinished()
    {
        return currentQuest != null && activeStage >= currentQuest.Stages.Length;
    }
    
    private void RefreshUI()
    {
        if (currentQuest == null) return;
        
        QuestStageDefinition stage = GetCurrentStage();
        if (stage != null)
        {
            if (objectiveField != null)
                objectiveField.text = stage.ObjectiveKey;
            
            if (progressField != null && stage.RequiredQuantity > 0)
                progressField.text = $"{stageProgress}/{stage.RequiredQuantity}";
            else if (progressField != null)
                progressField.text = "";
        }
    }
}