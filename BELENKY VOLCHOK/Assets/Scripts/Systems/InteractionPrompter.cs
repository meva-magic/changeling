using UnityEngine;

public class InteractionPrompter : MonoBehaviour
{
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private LayerMask detectableMask = -1;
    [SerializeField] private float updateThrottle = 0.1f;
    [SerializeField] private string defaultPromptKey = "hint.interaction";
    
    private Camera playerCamera;
    private GameObject currentTarget;
    private float lastUpdateTime;
    private string cachedPromptText;
    
    private void Start()
    {
        playerCamera = Camera.main;
        cachedPromptText = GetLocalizedText(defaultPromptKey);
    }
    
    private void Update()
    {
        MinigameStarter minigame = ServiceLocator.Get<MinigameStarter>();
        if (minigame != null && minigame.IsMinigameActive)
        {
            if (currentTarget != null) HidePrompt();
            return;
        }
        
        if (Time.time < lastUpdateTime + updateThrottle) return;
        lastUpdateTime = Time.time;
        
        if (playerCamera == null) return;
        
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, detectionRange, detectableMask))
        {
            IClickable interactable = hit.collider.GetComponent<IClickable>();
            if (interactable == null)
            {
                interactable = hit.collider.GetComponentInParent<IClickable>();
            }
            
            if (interactable != null)
            {
                if (currentTarget != hit.collider.gameObject)
                {
                    currentTarget = hit.collider.gameObject;
                    ShowPrompt();
                }
                return;
            }
        }
        
        if (currentTarget != null)
        {
            currentTarget = null;
            HidePrompt();
        }
    }
    
    private void ShowPrompt()
    {
        UserInterface ui = ServiceLocator.Get<UserInterface>();
        ui?.ShowHint(cachedPromptText);
    }
    
    private void HidePrompt()
    {
        UserInterface ui = ServiceLocator.Get<UserInterface>();
        ui?.HideHint();
    }
    
    private string GetLocalizedText(string key) { return key; }
}