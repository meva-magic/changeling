using UnityEngine;
using UnityEngine.Localization;
using TMPro;
using System.Collections;

public class ZoneBlocker : MonoBehaviour
{
    [Header("Block Settings")]
    [SerializeField] private string blockedItemID = "";
    [SerializeField] private string requiredItemID = "";
    
    [Header("Camera Target When Blocked")]
    [SerializeField] private Transform pushBackTarget; // Center of zone to push toward
    
    [Header("Reminder Settings")]
    [SerializeField] private LocalizedStringTable stringTable;
    [SerializeField] private string reminderLineKey;
    [SerializeField] private float reminderDuration = 2f;
    
    private bool reminderShowing;
    private Transform currentPlayer;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        currentPlayer = other.transform;
        
        PlayerCarry playerCarry = other.GetComponent<PlayerCarry>();
        
        if (ShouldBlockPlayer(playerCarry))
        {
            PushPlayerBack();
            ShowReminder();
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        currentPlayer = other.transform;
        
        PlayerCarry playerCarry = other.GetComponent<PlayerCarry>();
        
        if (ShouldBlockPlayer(playerCarry))
        {
            PushPlayerBack();
            ShowReminder();
        }
    }
    
    private bool ShouldBlockPlayer(PlayerCarry playerCarry)
    {
        if (playerCarry == null) return false;
        
        // Block if player HAS blocked item
        if (!string.IsNullOrEmpty(blockedItemID))
        {
            if (playerCarry.IsCarryingObject)
            {
                PickupableItem carriedItem = playerCarry.CarriedObject?.GetComponent<PickupableItem>();
                if (carriedItem != null && carriedItem.itemID == blockedItemID)
                {
                    return true;
                }
            }
        }
        
        // Block if player MISSING required item
        if (!string.IsNullOrEmpty(requiredItemID))
        {
            bool hasRequiredItem = false;
            
            if (playerCarry.IsCarryingObject)
            {
                PickupableItem carriedItem = playerCarry.CarriedObject?.GetComponent<PickupableItem>();
                if (carriedItem != null && carriedItem.itemID == requiredItemID)
                {
                    hasRequiredItem = true;
                }
            }
            
            if (!hasRequiredItem) return true;
        }
        
        return false;
    }
    
    private void PushPlayerBack()
    {
        if (currentPlayer == null) return;
        
        Vector3 pushDirection;
        
        if (pushBackTarget != null)
        {
            pushDirection = (pushBackTarget.position - currentPlayer.position).normalized;
        }
        else
        {
            pushDirection = (transform.position - currentPlayer.position).normalized;
        }
        
        currentPlayer.position += pushDirection * 2f;
    }
    
    private void ShowReminder()
    {
        if (reminderShowing) return;
        
        string text = GetLocalizedReminder();
        if (string.IsNullOrEmpty(text)) return;
        
        StartCoroutine(ShowReminderCoroutine(text));
    }
    
    private IEnumerator ShowReminderCoroutine(string text)
    {
        reminderShowing = true;
        
        if (SimpleDialogueManager.Instance != null)
            SimpleDialogueManager.Instance.ShowReminder(text);
        
        yield return new WaitForSeconds(reminderDuration);
        
        if (SimpleDialogueManager.Instance != null)
            SimpleDialogueManager.Instance.HideReminder();
        
        reminderShowing = false;
    }
    
    private string GetLocalizedReminder()
    {
        if (stringTable == null || string.IsNullOrEmpty(reminderLineKey)) return null;
        
        var table = stringTable.GetTable();
        if (table == null) return null;
        
        var entry = table[reminderLineKey];
        if (entry == null) return null;
        
        return entry.LocalizedValue;
    }
}