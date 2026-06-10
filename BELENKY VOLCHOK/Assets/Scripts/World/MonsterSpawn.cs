using UnityEngine;
using System.Collections.Generic;

public class MonsterSpawnManager : MonoBehaviour
{
    [SerializeField] private WindowMonsterPoint[] windows;
    [SerializeField] private float minSpawnDelay = 10f;
    [SerializeField] private float maxSpawnDelay = 20f;
    
    private WindowMonsterPoint currentMonster;
    private float nextSpawnTimer;
    
    private void Start()
    {
        for (int i = 0; i < windows.Length; i++)
        {
            if (windows[i] != null)
            {
                windows[i].WindowIndex = i;
            }
        }
        ScheduleNextSpawn();
    }
    
    private void Update()
    {
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
        nextSpawnTimer = Random.Range(minSpawnDelay, maxSpawnDelay);
    }
    
    private void SpawnAtRandomWindow()
    {
        List<WindowMonsterPoint> available = new List<WindowMonsterPoint>();
        foreach (WindowMonsterPoint window in windows)
        {
            if (window != null && !window.HasActiveMonster)
            {
                available.Add(window);
            }
        }
        
        if (available.Count == 0) return;
        
        currentMonster = available[Random.Range(0, available.Count)];
        currentMonster.SummonMonster();
    }
    
    public void ReportDefeat(WindowMonsterPoint defeated)
    {
        if (currentMonster == defeated)
        {
            currentMonster = null;
            ScheduleNextSpawn();
        }
    }
    
    public bool IsAnyMonsterActive
    {
        get { return currentMonster != null; }
    }
}