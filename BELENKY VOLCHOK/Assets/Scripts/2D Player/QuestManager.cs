using UnityEngine;
using TMPro;

public class SimpleQuestManager : MonoBehaviour
{
    public static SimpleQuestManager Instance;
    
    [SerializeField] private GameObject questPanel;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    
    private SimpleQuest activeQuest;
    private bool questCompleted;
    private bool isPanelHidden;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (questPanel != null)
            questPanel.SetActive(false);
    }
    
    public void StartQuest(SimpleQuest quest)
    {
        if (quest == null) return;
        
        activeQuest = quest;
        questCompleted = false;
        isPanelHidden = false;
        
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
        {
            string description = quest.GetDescription();
            if (!string.IsNullOrEmpty(description))
                questDescriptionText.text = description;
        }
    }
    
    public bool IsQuestActive(SimpleQuest quest)
    {
        if (quest == null) return false;
        return activeQuest == quest && !questCompleted;
    }
    
    public bool IsQuestPanelActive()
    {
        return questPanel != null && questPanel.activeSelf;
    }
    
    public void HideQuestPanel()
    {
        if (questPanel != null && questPanel.activeSelf)
        {
            questPanel.SetActive(false);
            isPanelHidden = true;
        }
    }
    
    public void ShowQuestPanel()
    {
        if (questPanel != null && isPanelHidden && activeQuest != null && !questCompleted)
        {
            questPanel.SetActive(true);
            isPanelHidden = false;
        }
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
        isPanelHidden = false;
        
        if (questPanel != null)
            questPanel.SetActive(false);
        
        activeQuest = null;
    }
}