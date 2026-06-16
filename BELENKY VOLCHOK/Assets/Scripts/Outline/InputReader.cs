using UnityEngine;

public class InputReader : MonoBehaviour
{
    public static InputReader Instance { get; private set; }
    
    [SerializeField] private bool allowMouseClick = true;
    [SerializeField] private string interactSound = "interact_press";
    [SerializeField] private float interactionCooldown = 0.2f;
    
    private Camera playerCamera;
    private float raycastRange = 5f;
    private LayerMask interactableMask = -1;
    private float lastActionTime;
    private SelectionManager selectionManager;
    
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
        playerCamera = Camera.main;
        selectionManager = FindObjectOfType<SelectionManager>();
    }
    
    private void Update()
    {
        MinigameStarter minigame = ServiceLocator.Get<MinigameStarter>();
        if (minigame != null && minigame.IsMinigameActive) return;
        
        if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsVisible) return;
        
        bool pressed = Input.GetKeyDown(KeyCode.Space) || (allowMouseClick && Input.GetMouseButtonDown(0));
        
        if (pressed && Time.time >= lastActionTime + interactionCooldown)
        {
            TryInteract();
        }
    }
    
    private void TryInteract()
    {
        if (selectionManager != null)
        {
            GameObject hoveredObject = selectionManager.GetHoveredObject();
            if (hoveredObject != null)
            {
                IClickable interactable = hoveredObject.GetComponent<IClickable>();
                if (interactable == null)
                    interactable = hoveredObject.GetComponentInParent<IClickable>();
                
                if (interactable != null)
                {
                    lastActionTime = Time.time;
                    
                    if (!string.IsNullOrEmpty(interactSound))
                        AudioManager.instance?.Play(interactSound);
                    
                    interactable.OnInteract();
                }
            }
        }
    }
}