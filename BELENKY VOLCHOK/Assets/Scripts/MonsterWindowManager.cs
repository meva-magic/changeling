using UnityEngine;
using System.Collections.Generic;

public class MonsterWindowManager : MonoBehaviour
{
    public static MonsterWindowManager Instance { get; private set; }
    
    [Header("Window Settings")]
    public WindowPoint[] windows;
    public float spawnMinInterval = 10f;
    public float spawnMaxInterval = 20f;
    
    private WindowPoint currentMonsterWindow;
    private float nextSpawnTimer;
    private bool spawningEnabled = true;
    
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
        for (int i = 0; i < windows.Length; i++)
        {
            if (windows[i] != null)
                windows[i].WindowId = i;
        }
        ScheduleNextSpawn();
    }
    
    private void Update()
    {
        if (!spawningEnabled) return;
        
        if (currentMonsterWindow == null && nextSpawnTimer > 0)
        {
            nextSpawnTimer -= Time.deltaTime;
            if (nextSpawnTimer <= 0)
            {
                SpawnMonsterAtRandomWindow();
            }
        }
    }
    
    private void ScheduleNextSpawn()
    {
        nextSpawnTimer = Random.Range(spawnMinInterval, spawnMaxInterval);
    }
    
    private void SpawnMonsterAtRandomWindow()
    {
        List<WindowPoint> available = new List<WindowPoint>();
        foreach (WindowPoint w in windows)
        {
            if (w != null && !w.HasActiveMonster)
                available.Add(w);
        }
        
        if (available.Count == 0) return;
        
        currentMonsterWindow = available[Random.Range(0, available.Count)];
        currentMonsterWindow.SpawnMonster();
    }
    
    public void OnMonsterDefeated(WindowPoint window)
    {
        if (currentMonsterWindow == window)
        {
            currentMonsterWindow = null;
            ScheduleNextSpawn();
        }
    }
    
    public bool HasActiveMonster()
    {
        return currentMonsterWindow != null;
    }
    
    public WindowPoint GetCurrentMonsterWindow()
    {
        return currentMonsterWindow;
    }
}