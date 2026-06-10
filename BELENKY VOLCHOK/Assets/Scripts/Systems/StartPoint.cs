using UnityEngine;

public class StartPoint : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private QuestJournal questJournal;
    [SerializeField] private MinigameStation minigameStation;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private CursorManager cursorManager;
    [SerializeField] private InteractionPrompter interactionPrompter;
    
    private void Awake()
    {
        if (audioManager != null) ServiceLocator.Assign<AudioManager>(audioManager);
        if (uiManager != null) ServiceLocator.Assign<UserInterface>(uiManager);
        if (questJournal != null) ServiceLocator.Assign<QuestTracker>(questJournal);
        if (minigameStation != null) ServiceLocator.Assign<MinigameStarter>(minigameStation);
        if (cursorManager != null) ServiceLocator.Assign<CursorController>(cursorManager);
        
        EventBus.Broadcast(GameEvents.GameStarted);
    }
}