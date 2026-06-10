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
    
    private Coroutine knockRoutine;
    
    public void SummonMonster()
    {
        if (HasActiveMonster) return;
        
        HasActiveMonster = true;
        if (monsterVisual != null)
        {
            monsterVisual.SetActive(true);
        }
        
        if (knockRoutine != null) StopCoroutine(knockRoutine);
        knockRoutine = StartCoroutine(PeriodicKnocks());
        
        EventBus.Broadcast(GameEvents.MonsterAppeared, this);
    }
    
    public void BanishMonster()
    {
        if (!HasActiveMonster) return;
        
        HasActiveMonster = false;
        if (monsterVisual != null)
        {
            monsterVisual.SetActive(false);
        }
        
        if (knockRoutine != null)
        {
            StopCoroutine(knockRoutine);
            knockRoutine = null;
        }
        
        EventBus.Broadcast(GameEvents.MonsterDefeated, this);
    }
    
    private IEnumerator PeriodicKnocks()
    {
        while (HasActiveMonster)
        {
            float wait = Random.Range(minKnockInterval, maxKnockInterval);
            yield return new WaitForSeconds(wait);
            
            if (HasActiveMonster && !string.IsNullOrEmpty(knockSoundId))
            {
                AudioManager.instance?.Play(knockSoundId);
            }
        }
    }
}