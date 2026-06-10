using UnityEngine;

public class InputReader : MonoBehaviour
{
    public static InputReader Instance { get; private set; }
    
    [Header("Key Bindings")]
    public KeyCode primaryKey = KeyCode.E;
    public KeyCode secondaryKey = KeyCode.Space;
    public bool allowMouseClick = true;
    
    [Header("Audio")]
    [SerializeField] private string interactSound = "interact_press";
    
    private Camera playerCamera;
    private float raycastRange = 5f;
    private LayerMask interactionMask = -1;
    private float lastActionTime;
    
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
    }
    
    private void Update()
    {
        MinigameStarter minigame = ServiceLocator.Get<MinigameStarter>();
        if (minigame != null && minigame.IsMinigameActive) return;
        
        // Диалог имеет приоритет
        if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsVisible)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                DialogueSystem.Instance.SkipOrClose();
                return;
            }
        }
        
        if (IsActionPressed() && Time.time >= lastActionTime + 0.1f)
        {
            AttemptInteraction();
        }
    }
    
    private bool IsActionPressed()
    {
        return Input.GetKeyDown(primaryKey) ||
               Input.GetKeyDown(secondaryKey) ||
               (allowMouseClick && Input.GetMouseButtonDown(0));
    }
    
    private void AttemptInteraction()
    {
        if (playerCamera == null) return;
        
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, raycastRange, interactionMask))
        {
            IClickable interactable = hit.collider.GetComponent<IClickable>();
            if (interactable == null)
            {
                interactable = hit.collider.GetComponentInParent<IClickable>();
            }
            
            if (interactable != null)
            {
                lastActionTime = Time.time;
                
                if (!string.IsNullOrEmpty(interactSound))
                {
                    AudioManager.instance?.Play(interactSound);
                }
                
                interactable.OnInteract();
            }
        }
    }
    
    public void SetRaycastRange(float range) { raycastRange = range; }
    public void SetInteractionMask(LayerMask mask) { interactionMask = mask; }
}