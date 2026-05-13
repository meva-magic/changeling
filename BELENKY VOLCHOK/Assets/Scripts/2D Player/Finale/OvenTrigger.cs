using UnityEngine;

public class OvenInteract : MonoBehaviour
{
    [SerializeField] private OvenClicker ovenClicker;
    [SerializeField] private GameObject interactIndicator;
    [SerializeField] private string requiredItemID = "Changeling";
    [SerializeField] private float interactRange = 2f;
    
    private bool playerInRange;
    private Transform player;
    
    private void Start()
    {
        if (interactIndicator != null) interactIndicator.SetActive(false);
    }
    
    private void Update()
    {
        if (!playerInRange || player == null) return;
        if (SimpleDialogueManager.Instance != null && SimpleDialogueManager.Instance.IsShowing) return;
        if (ovenClicker == null) return;
        
        // Show indicator if player has changeling
        PlayerCarry carry = player.GetComponent<PlayerCarry>();
        bool hasChangeling = carry != null && carry.IsCarryingObject && 
                            carry.CarriedObject != null &&
                            carry.CarriedObject.GetComponent<PickupableItem>()?.itemID == requiredItemID;
        
        if (interactIndicator != null)
            interactIndicator.SetActive(hasChangeling);
        
        if (hasChangeling && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            ovenClicker.StartMinigame();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.transform;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
            if (interactIndicator != null) interactIndicator.SetActive(false);
        }
    }
}