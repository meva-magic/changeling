using UnityEngine;
using TMPro;
using System.Collections;

public class SimpleQuestManager : MonoBehaviour
{
    public static SimpleQuestManager Instance;
    
    [SerializeField] private GameObject questPanel;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    
    private SimpleQuest activeQuest;
    private bool questCompleted;
    private float questTimer;
    private bool questHasTimer;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        questPanel.SetActive(false);
    }
    
    private void Update()
    {
        if (questHasTimer && activeQuest != null && !questCompleted)
        {
            questTimer -= Time.deltaTime;
            if (questTimer <= 0)
            {
                FailQuest();
            }
        }
    }
    
    public void StartQuest(string questID)
    {
        SimpleQuest quest = Resources.Load<SimpleQuest>($"Quests/{questID}");
        if (quest == null) return;
        
        activeQuest = quest;
        questCompleted = false;
        
        if (quest.questTimeLimit > 0)
        {
            questHasTimer = true;
            questTimer = quest.questTimeLimit;
        }
        
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
            if (item != null && item.itemID == activeQuest.requiredItemID)
                return true;
        }
        return false;
    }
    
    public void CompleteQuest(string questID)
    {
        if (activeQuest == null || activeQuest.questID != questID) return;
        
        questCompleted = true;
        questHasTimer = false;
        questPanel.SetActive(false);
        
        if (!string.IsNullOrEmpty(activeQuest.completionScene))
        {
            StartCoroutine(LoadSceneAfterDelay(activeQuest.completionScene, 1f));
        }
        
        activeQuest = null;
    }
    
    private void FailQuest()
    {
        questHasTimer = false;
        questCompleted = false;
        questPanel.SetActive(false);
        activeQuest = null;
    }
    
    private IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}