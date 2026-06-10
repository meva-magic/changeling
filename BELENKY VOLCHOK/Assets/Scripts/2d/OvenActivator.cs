using UnityEngine;

public class OvenActivator : MonoBehaviour
{
    [SerializeField] private OvenMinigame ovenMinigame;
    [SerializeField] private Quest requiredQuest;
    [SerializeField] private string requiredItemID = "Changeling";

    private bool playerInRange;
    private PlayerCarry playerCarry;

    private void Start()
    {
        if (ovenMinigame != null)
            ovenMinigame.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (ovenMinigame == null) return;

        bool shouldBeActive = IsQuestActive() && IsPlayerHoldingChangeling();
        
        if (shouldBeActive && !ovenMinigame.gameObject.activeSelf)
            ovenMinigame.gameObject.SetActive(true);
        else if (!shouldBeActive && ovenMinigame.gameObject.activeSelf && !ovenMinigame.IsCompleted)
            ovenMinigame.gameObject.SetActive(false);
    }

    private bool IsQuestActive()
    {
        if (QuestManager.Instance == null || requiredQuest == null) return false;
        return QuestManager.Instance.IsQuestActive(requiredQuest);
    }

    private bool IsPlayerHoldingChangeling()
    {
        if (playerCarry == null)
            playerCarry = FindObjectOfType<PlayerCarry>();

        if (playerCarry == null || !playerCarry.IsCarryingObject) return false;

        PickupableItem item = playerCarry.CarriedObject?.GetComponent<PickupableItem>();
        return item != null && item.itemID == requiredItemID;
    }
}