using UnityEngine;
using TMPro;

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
    
    [Header("Localization Keys")]
    public string interactionKeyTextKey = "interaction_key_hint";
    
    private Camera mainCamera;
    private GameObject currentTarget;
    private IInteractable currentInteractable;
    private float lastUpdateTime;
    
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
        
        // Update hint display - only one at a time
        if (hitObject != null && interactable != null)
        {
            if (currentTarget != hitObject)
            {
                currentTarget = hitObject;
                currentInteractable = interactable;
                ShowHint(interactable.GetInteractionName());
            }
        }
        else
        {
            if (currentTarget != null)
            {
                currentTarget = null;
                currentInteractable = null;
                HideHint();
            }
        }
    }
    
    private void ShowHint(string interactionName)
    {
        if (hintPanel != null && hintText != null)
        {
            string keyText = GetLocalizedText(interactionKeyTextKey);
            hintText.text = $"[{keyText}] {interactionName}";
            
            if (!hintPanel.activeSelf)
                hintPanel.SetActive(true);
        }
    }
    
    private void HideHint()
    {
        if (hintPanel != null && hintPanel.activeSelf)
            hintPanel.SetActive(false);
    }
    
    private string GetLocalizedText(string key)
    {
        var table = UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase;
        if (table != null && !string.IsNullOrEmpty(key))
            return table.GetLocalizedString("UI Table", key);
        return key;
    }
}