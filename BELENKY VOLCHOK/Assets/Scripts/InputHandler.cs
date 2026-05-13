using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }
    
    [Header("Settings")]
    public KeyCode interactionKey = KeyCode.E;
    public KeyCode alternateInteractionKey = KeyCode.Space;
    public bool allowMouseClick = true;
    
    private Camera mainCamera;
    private float interactionRange = 5f;
    private LayerMask interactableLayer = -1;
    private GameObject lastInteractedObject;
    private float interactionCooldown = 0.2f;
    private float lastInteractionTime;
    
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
    }
    
    private void Update()
    {
        // Don't process input if a minigame is active
        if (ClickerMinigameSystem.Instance != null && ClickerMinigameSystem.Instance.IsMinigameActive)
            return;
        
        // Check for interaction input
        bool interactionPressed = Input.GetKeyDown(interactionKey) ||
                                  Input.GetKeyDown(alternateInteractionKey) ||
                                  (allowMouseClick && Input.GetMouseButtonDown(0));
        
        if (interactionPressed && Time.time >= lastInteractionTime + interactionCooldown)
        {
            TryInteract();
        }
    }
    
    private void TryInteract()
    {
        if (mainCamera == null) return;
        
        // Raycast from center of screen
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            // Check for IInteractable on the hit object or its parent
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable == null)
                interactable = hit.collider.GetComponentInParent<IInteractable>();
            
            if (interactable != null)
            {
                // Prevent double interaction with same object
                if (lastInteractedObject == hit.collider.gameObject && Time.time < lastInteractionTime + 0.1f)
                    return;
                
                lastInteractedObject = hit.collider.gameObject;
                lastInteractionTime = Time.time;
                
                Debug.Log($"InputHandler: Interacting with {hit.collider.gameObject.name}");
                interactable.Interact();
            }
        }
    }
    
    public void SetInteractionRange(float range)
    {
        interactionRange = range;
    }
    
    public void SetInteractableLayer(LayerMask layer)
    {
        interactableLayer = layer;
    }
}