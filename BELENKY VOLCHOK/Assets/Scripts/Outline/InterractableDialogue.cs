using UnityEngine;

public class InteractableDialogue : MonoBehaviour, IClickable
{
    [Header("Dialogue Settings")]
    [SerializeField] private string dialogueKey = "default_dialogue";
    [SerializeField] private float interactionRange = 5f;
    [SerializeField] private string interactionSound = "interact_press";
    
    [Header("Dialogue Mode")]
    [SerializeField] private bool blockPlayerInput = true;
    [SerializeField] private float autoCloseDelay = 3f;
    
    [Header("Target Object for Outline")]
    [SerializeField] private GameObject outlineTarget;
    
    private Transform playerTransform;
    private bool isInRange;
    private Outline outlineComponent;
    private float lastInteractionTime;
    private float interactionCooldown = 0.5f;
    
    private GameObject EffectiveOutlineTarget
    {
        get { return outlineTarget != null ? outlineTarget : gameObject; }
    }
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
        
        outlineComponent = EffectiveOutlineTarget.GetComponent<Outline>();
        if (outlineComponent != null)
            outlineComponent.enabled = false;
    }
    
    private void Update()
    {
        if (playerTransform == null) return;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = isInRange;
        isInRange = distance <= interactionRange;
        
        if (outlineComponent != null)
        {
            if (isInRange && !wasInRange)
            {
                outlineComponent.OutlineColor = Color.white;
                outlineComponent.OutlineWidth = 0.05f;
                outlineComponent.enabled = true;
            }
            else if (!isInRange && wasInRange)
            {
                outlineComponent.enabled = false;
            }
        }
    }
    
    public void OnInteract()
    {
        if (!isInRange) return;
        
        if (Time.time < lastInteractionTime + interactionCooldown) return;
        lastInteractionTime = Time.time;
        
        if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsDialogueActive)
        {
            Debug.Log("InteractableDialogue: Диалог уже активен");
            return;
        }
        
        if (!string.IsNullOrEmpty(interactionSound))
            AudioManager.instance?.Play(interactionSound);
        
        DialogueSystem.Instance.SetBlockPlayerInput(blockPlayerInput);
        DialogueSystem.Instance.SetAutoCloseDelay(autoCloseDelay);
        
        DialogueSystem.Instance.ShowDialogue(dialogueKey, () => {
            Debug.Log($"Диалог {dialogueKey} завершён");
        });
    }
    
    public string GetPromptKey() { return ""; }
    public float GetInteractionRange() { return interactionRange; }
    public GameObject GetOutlineTarget() { return EffectiveOutlineTarget; }
}