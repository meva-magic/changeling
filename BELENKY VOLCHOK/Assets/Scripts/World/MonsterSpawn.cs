using UnityEngine;
using System.Collections.Generic;

public class MonsterSpawnManager : MonoBehaviour
{
    [SerializeField] private WindowMonsterPoint[] windows;
    [SerializeField] private float minSpawnDelay = 10f;
    [SerializeField] private float maxSpawnDelay = 20f;
    
    private WindowMonsterPoint currentMonster;
    private float nextSpawnTimer;
    private bool isSpawningEnabled = true;
    private int defeatedCount = 0;
    private bool doorUnlocked = false;
    private bool allMonstersDefeated = false;
    private int requiredWins = 0;
    
    private void Start()
    {
        for (int i = 0; i < windows.Length; i++)
        {
            if (windows[i] != null)
                windows[i].WindowIndex = i;
        }
        requiredWins = windows.Length;
        ScheduleNextSpawn();
    }
    
    private void OnEnable()
    {
        EventBus.Listen(GameEvents.MonsterDefeated, OnMonsterDefeated);
    }
    
    private void OnDisable()
    {
        EventBus.StopListening(GameEvents.MonsterDefeated, OnMonsterDefeated);
    }
    
    private void Update()
    {
        if (!isSpawningEnabled || doorUnlocked || allMonstersDefeated) return;
        
        if (currentMonster == null && nextSpawnTimer > 0)
        {
            nextSpawnTimer -= Time.deltaTime;
            if (nextSpawnTimer <= 0)
            {
                SpawnAtRandomWindow();
            }
        }
    }
    
    private void ScheduleNextSpawn()
    {
        if (allMonstersDefeated) return;
        nextSpawnTimer = Random.Range(minSpawnDelay, maxSpawnDelay);
        Debug.Log($"MonsterSpawnManager: Следующий спавн через {nextSpawnTimer} секунд");
    }
    
    private void SpawnAtRandomWindow()
    {
        List<WindowMonsterPoint> available = new List<WindowMonsterPoint>();
        foreach (WindowMonsterPoint window in windows)
        {
            if (window != null && !window.HasActiveMonster && !window.IsDefeated && !window.IsUsed)
                available.Add(window);
        }
        
        if (available.Count == 0)
        {
            Debug.Log("MonsterSpawnManager: Нет доступных окон для спавна, все монстры побеждены");
            allMonstersDefeated = true;
            isSpawningEnabled = false;
            return;
        }
        
        currentMonster = available[Random.Range(0, available.Count)];
        currentMonster.SummonMonster();
        Debug.Log($"MonsterSpawnManager: Монстр появился на окне {currentMonster.WindowIndex}");
    }
    
    private void OnMonsterDefeated(object data)
    {
        WindowMonsterPoint defeated = data as WindowMonsterPoint;
        if (defeated != null && defeated == currentMonster)
        {
            defeatedCount++;
            Debug.Log($"MonsterSpawnManager: Монстр на окне {defeated.WindowIndex} побеждён ({defeatedCount}/{requiredWins})");
            currentMonster = null;
            
            // СТОП — МГНОВЕННО, как только достигнуто нужное количество
            if (defeatedCount >= requiredWins)
            {
                Debug.Log("MonsterSpawnManager: Все монстры побеждены! Спавн МГНОВЕННО остановлен");
                allMonstersDefeated = true;
                isSpawningEnabled = false;
                
                // Отправляем событие, что все монстры побеждены
                EventBus.Broadcast("AllMonstersDefeated");
                return;
            }
            
            ScheduleNextSpawn();
        }
    }
    
    public void BanishAllMonsters()
    {
        foreach (WindowMonsterPoint window in windows)
        {
            if (window != null && window.HasActiveMonster)
            {
                window.BanishMonster();
            }
            if (window != null)
            {
                window.SetDoorUnlocked(true);
            }
        }
        currentMonster = null;
        isSpawningEnabled = false;
        Debug.Log("MonsterSpawnManager: Все монстры изгнаны");
    }
    
    public void UnlockDoor()
    {
        doorUnlocked = true;
        isSpawningEnabled = false;
        BanishAllMonsters();
        Debug.Log("MonsterSpawnManager: Дверь разблокирована, спавн монстров остановлен");
    }
    
    public void DisableSpawning() 
    { 
        isSpawningEnabled = false; 
        Debug.Log("MonsterSpawnManager: Спавн монстров отключен");
    }
    
    public void EnableSpawning() 
    { 
        isSpawningEnabled = true; 
    }
}