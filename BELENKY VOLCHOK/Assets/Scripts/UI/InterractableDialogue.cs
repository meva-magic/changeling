using UnityEngine;

public class InteractableDialogue : MonoBehaviour, IClickable
{
    [SerializeField] private string dialogueKey = "default_dialogue";
    [SerializeField] private float interactionRange = 2.5f;
    [SerializeField] private string interactionSound = "interact_press";
    
    [Header("Outline")]
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float outlineWidth = 0.05f;
    
    private Transform playerTransform;
    private bool isInRange;
    private Outline outlineComponent;
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
        
        outlineComponent = GetComponent<Outline>();
        if (outlineComponent != null)
            outlineComponent.enabled = false;
    }
    
    private void Update()
    {
        if (playerTransform == null) return;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        isInRange = distance <= interactionRange;
        
        if (outlineComponent != null)
        {
            outlineComponent.enabled = isInRange;
            if (outlineComponent.enabled)
            {
                outlineComponent.OutlineColor = outlineColor;
                outlineComponent.OutlineWidth = outlineWidth;
            }
        }
    }
    
    public void OnInteract()
    {
        if (!isInRange) return;
        
        if (!string.IsNullOrEmpty(interactionSound))
            AudioManager.instance?.Play(interactionSound);
        
        if (DialogueSystem.Instance != null && !DialogueSystem.Instance.IsVisible)
        {
            PlayerMovement movement = playerTransform?.GetComponent<PlayerMovement>();
            if (movement != null)
                movement.SetMovementEnabled(false);
            
            DialogueSystem.Instance.ShowDialogue(dialogueKey, () => {
                if (movement != null)
                    movement.SetMovementEnabled(true);
            });
        }
    }
    
    public string GetPromptKey()
    {
        return "";
    }
    
    public float GetInteractionRange()
    {
        return interactionRange;
    }
}