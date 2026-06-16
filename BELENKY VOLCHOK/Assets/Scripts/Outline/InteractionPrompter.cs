using UnityEngine;

public class InteractionPrompter : MonoBehaviour
{
    public static InteractionPrompter Instance { get; private set; }
    
    [SerializeField] private string promptKey = "hint.interaction";
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private LayerMask interactableLayer = -1;
    
    private string cachedPromptText;
    private Camera mainCamera;
    private Transform playerTransform;
    private bool isHintVisible = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        mainCamera = Camera.main;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
        cachedPromptText = GetLocalizedText(promptKey);
    }
    
    private void Update()
    {
        MinigameStarter minigame = ServiceLocator.Get<MinigameStarter>();
        bool isMinigameActive = minigame != null && minigame.IsMinigameActive;
        bool isDialogueActive = DialogueSystem.Instance != null && DialogueSystem.Instance.IsDialogueActive;
        
        if (isMinigameActive || isDialogueActive)
        {
            if (isHintVisible) HideHint();
            return;
        }
        
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, detectionRange, interactableLayer))
        {
            IClickable interactable = hit.collider.GetComponent<IClickable>();
            if (interactable == null)
                interactable = hit.collider.GetComponentInParent<IClickable>();
            
            if (interactable != null)
            {
                float range = interactable.GetInteractionRange();
                float distanceToPlayer = Vector3.Distance(hit.collider.transform.position, playerTransform.position);
                
                if (hit.distance <= range && distanceToPlayer <= range)
                {
                    if (!isHintVisible)
                    {
                        ShowHint();
                    }
                    return;
                }
            }
        }
        
        if (isHintVisible)
        {
            HideHint();
        }
    }
    
    private void ShowHint()
    {
        UserInterface ui = ServiceLocator.Get<UserInterface>();
        if (ui != null)
        {
            ui.ShowHint(cachedPromptText);
            isHintVisible = true;
        }
    }
    
    private void HideHint()
    {
        UserInterface ui = ServiceLocator.Get<UserInterface>();
        if (ui != null)
        {
            ui.HideHint();
            isHintVisible = false;
        }
    }
    
    private string GetLocalizedText(string key)
    {
        return key;
    }
}