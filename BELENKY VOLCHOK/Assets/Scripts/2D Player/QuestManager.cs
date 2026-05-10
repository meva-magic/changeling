using UnityEngine;
using TMPro;

public class SimpleQuestManager : MonoBehaviour
{
    public static SimpleQuestManager Instance;
    
    [SerializeField] private GameObject questPanel;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    
    private SimpleQuest activeQuest;
    private bool questCompleted;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (questPanel != null)
            questPanel.SetActive(false);
    }
    
    public void StartQuest(SimpleQuest quest)
    {
        if (quest == null)
        {
            Debug.LogError("Quest is null!");
            return;
        }
        
        activeQuest = quest;
        questCompleted = false;
        
        if (questPanel != null)
        {
            questPanel.SetActive(true);
            
            Transform parent = questPanel.transform.parent;
            while (parent != null)
            {
                if (!parent.gameObject.activeSelf)
                    parent.gameObject.SetActive(true);
                parent = parent.parent;
            }
        }
        
        if (questDescriptionText != null)
            questDescriptionText.text = quest.description;
    }
    
    public bool IsQuestActive(SimpleQuest quest)
    {
        if (quest == null) return false;
        return activeQuest == quest && !questCompleted;
    }
    
    public bool CanCompleteQuest(SimpleQuest quest)
    {
        if (quest == null || activeQuest != quest) return false;
        
        PlayerCarry playerCarry = FindObjectOfType<PlayerCarry>();
        if (playerCarry != null && playerCarry.IsCarryingObject)
        {
            PickupableItem item = playerCarry.CarriedObject?.GetComponent<PickupableItem>();
            if (item != null && item.itemID == quest.requiredItemID)
                return true;
        }
        return false;
    }
    
    public void CompleteQuest(SimpleQuest quest)
    {
        if (quest == null || activeQuest != quest) return;
        
        questCompleted = true;
        
        if (questPanel != null)
            questPanel.SetActive(false);
        
        activeQuest = null;
    }
}