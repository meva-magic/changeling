using UnityEngine;

public class Firewood : MonoBehaviour, IClickable
{
    [SerializeField] private string targetTag = "Firewood";
    [SerializeField] private float pickupRange = 2f;
    
    private bool wasCollected;
    private Transform playerTransform;
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }
    
    public void OnInteract()
    {
        if (!IsPlayerInRange()) return;
        if (wasCollected) return;
        
        QuestTracker quest = ServiceLocator.Get<QuestTracker>();
        if (quest == null) return;
        
        QuestStageDefinition stage = quest.GetCurrentStage();
        if (stage != null && stage.RequiredTag == targetTag && stage.RequiredQuantity > 0)
        {
            wasCollected = true;
            quest.RecordCollectedItem(targetTag);
            Destroy(gameObject);
        }
        else
        {
            UserInterface ui = ServiceLocator.Get<UserInterface>();
            ui?.ShowMessage("need_gather_wood");
        }
    }
    
    private bool IsPlayerInRange()
    {
        if (playerTransform == null) return true;
        return Vector3.Distance(transform.position, playerTransform.position) <= pickupRange;
    }
    
    public string GetPromptKey()
    {
        return "";
    }
}