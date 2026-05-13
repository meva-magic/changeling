using UnityEngine;

public class FirewoodCollectible : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public string itemTag = "Firewood";
    
    private bool collected;
    
    public void Interact()
    {
        Debug.Log($"Firewood Interact called on {gameObject.name}");
        
        if (collected)
        {
            Debug.Log("Already collected");
            return;
        }
        
        if (CanCollect())
        {
            collected = true;
            Debug.Log("Collecting firewood");
            QuestManager.Instance?.CollectItem(itemTag, 1);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Cannot collect - not in correct quest stage");
            ShowMessage("need_gather_wood");
        }
    }
    
    private bool CanCollect()
    {
        if (QuestManager.Instance == null)
        {
            Debug.Log("No QuestManager found");
            return true;
        }
        
        var stage = QuestManager.Instance.GetCurrentStage();
        if (stage == null)
        {
            Debug.Log("No current stage");
            return false;
        }
        
        Debug.Log($"CanCollect: stage.targetTag={stage.targetTag}, stage.requiredAmount={stage.requiredAmount}");
        return stage.targetTag == itemTag && stage.requiredAmount > 0;
    }
    
    private void ShowMessage(string key)
    {
        if (UIMessageManager.Instance != null)
            UIMessageManager.Instance.ShowMessage(key);
    }
    
    public string GetInteractionName()
    {
        return "Firewood";
    }
}