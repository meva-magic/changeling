using UnityEngine;

public class WindowPoint : MonoBehaviour
{
    [Header("Visuals")]
    public GameObject monsterModel;
    
    public int WindowId { get; set; }
    public bool HasActiveMonster { get; private set; }
    
    public void SpawnMonster()
    {
        if (HasActiveMonster) return;
        
        HasActiveMonster = true;
        
        if (monsterModel != null)
            monsterModel.SetActive(true);
    }
    
    public void DespawnMonster()
    {
        if (!HasActiveMonster) return;
        
        HasActiveMonster = false;
        
        if (monsterModel != null)
            monsterModel.SetActive(false);
        
        MonsterWindowManager.Instance?.OnMonsterDefeated(this);
    }
}