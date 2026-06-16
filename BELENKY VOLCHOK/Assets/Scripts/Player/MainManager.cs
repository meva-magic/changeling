using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }
    
    [Header("Progress")]
    [SerializeField] private int requiredCurtainWins = 3;
    [SerializeField] private float doorUnlockDelay = 2f;
    
    [Header("Door Settings")]
    [SerializeField] private DoorRiddleMinigame doorRiddle;
    [SerializeField] private GameObject doorBlocker;
    
    private int curtainWinsCount = 0;
    private bool isDoorUnlocked = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        if (doorBlocker != null) doorBlocker.SetActive(true);
        if (doorRiddle != null) doorRiddle.enabled = false;
    }
    
    private void OnEnable()
    {
        EventBus.Listen(GameEvents.MonsterDefeated, OnCurtainWin);
    }
    
    private void OnDisable()
    {
        EventBus.StopListening(GameEvents.MonsterDefeated, OnCurtainWin);
    }
    
    private void OnCurtainWin(object data)
    {
        curtainWinsCount++;
        Debug.Log($"SceneManager: Победа над монстром {curtainWinsCount}/{requiredCurtainWins}");
        
        if (curtainWinsCount >= requiredCurtainWins && !isDoorUnlocked)
        {
            UnlockDoor();
        }
    }
    
    private void UnlockDoor()
    {
        isDoorUnlocked = true;
        Debug.Log($"SceneManager: Дверь разблокирована через {doorUnlockDelay} секунд");
        
        Invoke(nameof(ActivateDoor), doorUnlockDelay);
    }
    
    private void ActivateDoor()
    {
        if (doorBlocker != null) doorBlocker.SetActive(false);
        if (doorRiddle != null) doorRiddle.enabled = true;
        
        UserInterface ui = ServiceLocator.Get<UserInterface>();
        ui?.ShowMessage("door_unlocked", 3f);
    }
}