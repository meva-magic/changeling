using UnityEngine;
using System.Collections;

public class WindowMonsterPoint : MonoBehaviour
{
    [SerializeField] private GameObject monsterVisual;
    [SerializeField] private string knockSoundId = "window_tap";
    [SerializeField] private float minKnockInterval = 3f;
    [SerializeField] private float maxKnockInterval = 8f;
    
    public int WindowIndex { get; set; }
    public bool HasActiveMonster { get; private set; }
    public bool IsDefeated { get; private set; }
    public bool IsUsed { get; private set; }
    private Coroutine knockRoutine;
    private bool counterAdded = false;
    private bool isDoorUnlocked = false;
    
    public void SetDoorUnlocked(bool unlocked)
    {
        isDoorUnlocked = unlocked;
        if (isDoorUnlocked && HasActiveMonster)
        {
            BanishMonster();
        }
    }
    
    public void SummonMonster()
    {
        if (isDoorUnlocked) return;
        if (HasActiveMonster || IsDefeated || IsUsed) return;
        HasActiveMonster = true;
        if (monsterVisual != null) monsterVisual.SetActive(true);
        if (knockRoutine != null) StopCoroutine(knockRoutine);
        knockRoutine = StartCoroutine(PeriodicKnocks());
        
        if (!counterAdded)
        {
            counterAdded = true;
            ThreatSystem.Instance?.AddCounterSource();
        }
        
        EventBus.Broadcast(GameEvents.MonsterAppeared, this);
    }
    
    public void BanishMonster()
    {
        if (!HasActiveMonster) return;
        HasActiveMonster = false;
        IsDefeated = true;
        IsUsed = true;
        
        if (monsterVisual != null) monsterVisual.SetActive(false);
        if (knockRoutine != null)
        {
            StopCoroutine(knockRoutine);
            knockRoutine = null;
        }
        if (counterAdded)
        {
            counterAdded = false;
            ThreatSystem.Instance?.RemoveCounterSource();
        }
        
        EventBus.Broadcast(GameEvents.MonsterDefeated, this);
        Debug.Log($"WindowMonsterPoint {WindowIndex}: Монстр изгнан навсегда");
    }
    
    private IEnumerator PeriodicKnocks()
    {
        while (HasActiveMonster)
        {
            float wait = Random.Range(minKnockInterval, maxKnockInterval);
            yield return new WaitForSeconds(wait);
            if (HasActiveMonster && !string.IsNullOrEmpty(knockSoundId))
                AudioManager.instance?.Play(knockSoundId);
        }
    }
}