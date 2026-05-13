using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Localization;

public class InteractionMessage : MonoBehaviour
{
    public static InteractionMessage Instance { get; set; }
    
    [Header("UI References")]
    public GameObject messagePanel;
    public TextMeshProUGUI messageText;
    
    [Header("Settings")]
    public float messageDisplayDuration = 2f;
    public float maxInteractionRange = 5f;
    public LayerMask interactableLayer = -1;
    
    [Header("Localization")]
    public LocalizedStringTable stringTable; // This gives you a dropdown!
    
    private Camera mainCamera;
    private Coroutine hideCoroutine;
    private bool isShowing;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (messagePanel != null)
            messagePanel.SetActive(false);
    }
    
    private void Start()
    {
        mainCamera = Camera.main;
    }
    
    private void Update()
    {
        // Don't show during minigame
        if (ClickerMinigameSystem.Instance != null && ClickerMinigameSystem.Instance.IsMinigameActive)
            return;
        
        // Check for interaction input
        bool interactionPressed = Input.GetKeyDown(KeyCode.E) || 
                                  Input.GetKeyDown(KeyCode.Space) || 
                                  Input.GetMouseButtonDown(0);
        
        if (interactionPressed)
        {
            TryShowInteractionMessage();
        }
    }
    
    private void TryShowInteractionMessage()
    {
        if (mainCamera == null) return;
        
        // Raycast from center of screen
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxInteractionRange, interactableLayer))
        {
            // Check for IInteractable
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable == null)
                interactable = hit.collider.GetComponentInParent<IInteractable>();
            
            if (interactable != null)
            {
                // Get the message key based on object type
                string messageKey = GetMessageKeyForObject(hit.collider.gameObject);
                ShowMessage(messageKey);
            }
        }
    }
    
    private string GetMessageKeyForObject(GameObject obj)
    {
        // Check by tag or component type
        if (obj.CompareTag("Firewood") || obj.GetComponent<FirewoodCollectible>() != null)
        {
            return "message_firewood";
        }
        
        if (obj.GetComponent<FireplaceInteract>() != null)
        {
            return "message_fireplace";
        }
        
        if (obj.GetComponent<CurtainController>() != null)
        {
            return "message_curtain";
        }
        
        if (obj.GetComponent<CandleSystem>() != null)
        {
            return "message_candle";
        }
        
        // Default message
        return "message_interact";
    }
    
    public void ShowMessage(string messageKey)
    {
        // Don't show if a minigame is active
        if (ClickerMinigameSystem.Instance != null && ClickerMinigameSystem.Instance.IsMinigameActive)
            return;
        
        // Don't show if already showing
        if (isShowing) return;
        
        string localizedMessage = GetLocalizedText(messageKey);
        
        if (string.IsNullOrEmpty(localizedMessage))
            return;
        
        if (messageText != null)
            messageText.text = localizedMessage;
        
        if (messagePanel != null)
            messagePanel.SetActive(true);
        
        isShowing = true;
        
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);
        
        hideCoroutine = StartCoroutine(AutoHide());
    }
    
    private IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(messageDisplayDuration);
        
        if (messagePanel != null)
            messagePanel.SetActive(false);
        
        isShowing = false;
    }
    
    public void HideMessage()
    {
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);
        
        if (messagePanel != null)
            messagePanel.SetActive(false);
        
        isShowing = false;
    }
    
    private string GetLocalizedText(string key)
    {
        if (stringTable.IsEmpty)
        {
            Debug.LogWarning("String Table not assigned in InteractionMessage! Please assign in inspector.");
            return key;
        }
        
        var table = stringTable.GetTable();
        if (table != null)
        {
            var entry = table.GetEntry(key);
            if (entry != null)
            {
                return entry.GetLocalizedString();
            }
        }
        
        Debug.LogWarning($"Key '{key}' not found in String Table");
        return key;
    }
}