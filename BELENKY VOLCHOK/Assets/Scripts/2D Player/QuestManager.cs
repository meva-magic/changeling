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
        questPanel.SetActive(false);
    }
    
    public void StartQuest(string questID)
    {
        SimpleQuest quest = Resources.Load<SimpleQuest>($"Quests/{questID}");
        if (quest == null) return;
        activeQuest = quest;
        questCompleted = false;
        questPanel.SetActive(true);
        questDescriptionText.text = quest.description;
    }
    
    public bool IsQuestActive(string questID)
    {
        return activeQuest != null && activeQuest.questID == questID && !questCompleted;
    }
    
    public bool CanCompleteQuest(string questID)
    {
        if (activeQuest == null || activeQuest.questID != questID) return false;
        PlayerCarry playerCarry = FindObjectOfType<PlayerCarry>();
        if (playerCarry != null && playerCarry.IsCarryingObject)
        {
            PickupableItem item = playerCarry.CarriedObject?.GetComponent<PickupableItem>();
            if (item != null && item.itemID == activeQuest.requiredItemID) return true;
        }
        return false;
    }
    
    public void CompleteQuest(string questID)
    {
        if (activeQuest == null || activeQuest.questID != questID) return;
        questCompleted = true;
        questPanel.SetActive(false);
        activeQuest = null;
    }
}