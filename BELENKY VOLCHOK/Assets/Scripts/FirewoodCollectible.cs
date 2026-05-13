using UnityEngine;

public class FirewoodCollectible : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public string itemTag = "Firewood";
    public float interactionRange = 2f;
    
    private bool collected;
    private Transform playerTransform;
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }
    
    public void Interact()
    {
        if (!IsInRange())
        {
            Debug.Log("Too far from firewood");
            return;
        }
        
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
    
    private bool IsInRange()
    {
        if (playerTransform == null) return true;
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= interactionRange;
    }
    
    private bool CanCollect()
    {
        if (QuestManager.Instance == null) return true;
        
        var stage = QuestManager.Instance.GetCurrentStage();
        if (stage == null) return false;
        
        return stage.targetTag == itemTag && stage.requiredAmount > 0;
    }
    
    private void ShowMessage(string key)
    {
        if (UIMessageManager.Instance != null)
            UIMessageManager.Instance.ShowMessage(key);
    }
    
    public string GetInteractionName()
    {
        return ""; // No interaction name
    }
}