using UnityEngine;
using TMPro;
using UnityEngine.Localization;

public class InteractionHint : MonoBehaviour
{
    public static InteractionHint Instance { get; set; }
    
    [Header("UI References")]
    public GameObject hintPanel;
    public TextMeshProUGUI hintText;
    
    [Header("Settings")]
    public float maxInteractionRange = 5f;
    public LayerMask interactableLayer = -1;
    public float hintUpdateDelay = 0.1f;
    
    [Header("Localization")]
    public LocalizedStringTable stringTable; // This gives you a dropdown!
    public string interactionKey = "hint.interaction";
    
    private Camera mainCamera;
    private GameObject currentTarget;
    private float lastUpdateTime;
    private string cachedLocalizedText = "";
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }
    
    private void Start()
    {
        mainCamera = Camera.main;
        LoadLocalizedText();
    }
    
    private void LoadLocalizedText()
    {
        if (stringTable.IsEmpty)
        {
            Debug.LogWarning("String Table not assigned in InteractionHint! Please assign in inspector.");
            cachedLocalizedText = "E / Space / Mouse";
            return;
        }
        
        var table = stringTable.GetTable();
        if (table != null)
        {
            var entry = table.GetEntry(interactionKey);
            if (entry != null)
            {
                cachedLocalizedText = entry.GetLocalizedString();
                Debug.Log($"Localized text loaded: {cachedLocalizedText}");
            }
            else
            {
                Debug.LogWarning($"Key '{interactionKey}' not found in String Table");
                cachedLocalizedText = "E / Space / Mouse";
            }
        }
        else
        {
            Debug.LogWarning("Could not load String Table");
            cachedLocalizedText = "E / Space / Mouse";
        }
    }
    
    private void Update()
    {
        // Don't show hints during minigame
        if (ClickerMinigameSystem.Instance != null && ClickerMinigameSystem.Instance.IsMinigameActive)
        {
            if (currentTarget != null)
                HideHint();
            return;
        }
        
        // Limit update frequency
        if (Time.time < lastUpdateTime + hintUpdateDelay)
            return;
        lastUpdateTime = Time.time;
        
        if (mainCamera == null) return;
        
        // Raycast to find interactable object
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;
        
        GameObject hitObject = null;
        IInteractable interactable = null;
        
        if (Physics.Raycast(ray, out hit, maxInteractionRange, interactableLayer))
        {
            interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable == null)
                interactable = hit.collider.GetComponentInParent<IInteractable>();
            
            if (interactable != null)
            {
                hitObject = hit.collider.gameObject;
            }
        }
        
        // Update hint display - show only key hint, no object name
        if (hitObject != null && interactable != null)
        {
            if (currentTarget != hitObject)
            {
                currentTarget = hitObject;
                ShowHint();
            }
        }
        else
        {
            if (currentTarget != null)
            {
                currentTarget = null;
                HideHint();
            }
        }
    }
    
    private void ShowHint()
    {
        if (hintPanel != null && hintText != null)
        {
            hintText.text = $"[{cachedLocalizedText}]";
            
            if (!hintPanel.activeSelf)
                hintPanel.SetActive(true);
        }
    }
    
    private void HideHint()
    {
        if (hintPanel != null && hintPanel.activeSelf)
            hintPanel.SetActive(false);
    }
    
    public void RefreshLocalization()
    {
        LoadLocalizedText();
    }
}