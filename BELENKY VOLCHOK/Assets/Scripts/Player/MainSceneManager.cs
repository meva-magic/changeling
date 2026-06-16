using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }
    
    [Header("Progress")]
    [SerializeField] private int requiredCurtainWins = 3;
    [SerializeField] private float doorUnlockDelay = 2f;
    
    [Header("Door Settings")]
    [SerializeField] private GameObject inactiveDoor;
    [SerializeField] private GameObject activeDoor;
    [SerializeField] private MonsterSpawnManager spawnManager;
    [SerializeField] private CandleSystem candleSystem;
    
    private int curtainWinsCount = 0;
    private bool isDoorUnlocked = false;
    private bool isDoorActivated = false;
    private bool allMonstersDefeated = false;
    
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
        if (inactiveDoor != null) inactiveDoor.SetActive(true);
        if (activeDoor != null) activeDoor.SetActive(false);
    }
    
    private void OnEnable()
    {
        EventBus.Listen(GameEvents.MonsterDefeated, OnCurtainWin);
        EventBus.Listen("AllMonstersDefeated", OnAllMonstersDefeated);
    }
    
    private void OnDisable()
    {
        EventBus.StopListening(GameEvents.MonsterDefeated, OnCurtainWin);
        EventBus.StopListening("AllMonstersDefeated", OnAllMonstersDefeated);
    }
    
    private void OnAllMonstersDefeated()
    {
        if (allMonstersDefeated) return;
        allMonstersDefeated = true;
        Debug.Log("SceneManager: Все монстры побеждены! Начинаем отсчёт до открытия двери");
        
        if (candleSystem != null)
        {
            candleSystem.SwitchToAlternativeCandle();
        }
        
        UnlockDoor();
    }
    
    private void OnCurtainWin(object data)
    {
        if (isDoorUnlocked) return;
        if (allMonstersDefeated) return;
        
        curtainWinsCount++;
        Debug.Log($"SceneManager: Победа над монстром {curtainWinsCount}/{requiredCurtainWins}");
        
        if (curtainWinsCount >= requiredCurtainWins)
        {
            // Отправляем событие, что все монстры побеждены
            EventBus.Broadcast("AllMonstersDefeated");
        }
    }
    
    private void UnlockDoor()
    {
        if (isDoorUnlocked) return;
        isDoorUnlocked = true;
        
        Debug.Log($"SceneManager: Дверь разблокируется через {doorUnlockDelay} секунд");
        
        if (spawnManager != null)
        {
            spawnManager.DisableSpawning();
            spawnManager.BanishAllMonsters();
        }
        
        Invoke(nameof(ActivateDoor), doorUnlockDelay);
    }
    
    private void ActivateDoor()
    {
        if (isDoorActivated) return;
        isDoorActivated = true;
        
        if (inactiveDoor != null) inactiveDoor.SetActive(false);
        if (activeDoor != null) activeDoor.SetActive(true);
        
        DoorRiddleMinigame door = activeDoor?.GetComponent<DoorRiddleMinigame>();
        if (door != null) door.SetUnlocked(true);
        
        Debug.Log("SceneManager: Дверь активирована");
    }
}