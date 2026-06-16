using UnityEngine;

public class Firewood : MonoBehaviour, IClickable
{
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private GameObject outlineTarget;
    
    private bool wasCollected;
    
    private GameObject EffectiveOutlineTarget
    {
        get { return outlineTarget != null ? outlineTarget : gameObject; }
    }
    
    public void OnInteract()
    {
        if (wasCollected) return;
        
        if (!IsPlayerInRange())
        {
            Debug.Log("Слишком далеко от дров");
            return;
        }
        
        QuestTracker quest = ServiceLocator.Get<QuestTracker>();
        if (quest == null) return;
        
        QuestStageDefinition stage = quest.GetCurrentStage();
        if (stage != null && stage.RequiredTag == "Firewood" && stage.RequiredQuantity > 0)
        {
            wasCollected = true;
            quest.RecordCollectedItem("Firewood");
            
            RemoveOutline();
            Destroy(gameObject);
        }
        else
        {
            UserInterface ui = ServiceLocator.Get<UserInterface>();
            ui?.ShowMessage("need_gather_wood");
        }
    }
    
    private void RemoveOutline()
    {
        Outline outline = EffectiveOutlineTarget.GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;
    }
    
    private bool IsPlayerInRange()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return true;
        
        float distance = Vector3.Distance(transform.position, player.transform.position);
        return distance <= interactionRange;
    }
    
    public string GetPromptKey() { return ""; }
    public float GetInteractionRange() { return interactionRange; }
    public GameObject GetOutlineTarget() { return EffectiveOutlineTarget; }
}