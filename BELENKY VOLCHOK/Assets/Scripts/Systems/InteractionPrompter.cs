using UnityEngine;

public class InteractionPrompter : MonoBehaviour
{
    public static InteractionPrompter Instance { get; private set; }
    
    [SerializeField] private string promptKey = "hint.interaction";
    
    private string cachedPromptText;
    private UserInterface userInterface;
    private SelectionManager selectionManager;
    private GameObject currentTarget;
    private bool isPromptVisible = false;
    private float checkTimer = 0f;
    private GameObject lastValidTarget;
    
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
        cachedPromptText = GetLocalizedText(promptKey);
        userInterface = ServiceLocator.Get<UserInterface>();
        selectionManager = FindObjectOfType<SelectionManager>();
    }
    
    private void Update()
    {
        MinigameStarter minigame = ServiceLocator.Get<MinigameStarter>();
        if (minigame != null && minigame.IsMinigameActive)
        {
            if (isPromptVisible) HidePrompt();
            currentTarget = null;
            lastValidTarget = null;
            return;
        }
        
        // Проверяем каждые 0.1 секунды
        checkTimer += Time.deltaTime;
        if (checkTimer < 0.1f) return;
        checkTimer = 0f;
        
        GameObject newTarget = null;
        
        if (selectionManager != null)
        {
            newTarget = selectionManager.GetHoveredObject();
        }
        
        // Если текущий target уничтожен — сбрасываем
        if (currentTarget != null && currentTarget == null)
        {
            currentTarget = null;
            lastValidTarget = null;
            if (isPromptVisible) HidePrompt();
        }
        
        // Проверяем валидность нового target
        bool isValid = newTarget != null && IsValidInteractable(newTarget);
        
        if (isValid)
        {
            if (newTarget != currentTarget)
            {
                currentTarget = newTarget;
                if (!isPromptVisible) ShowPrompt();
            }
        }
        else
        {
            if (currentTarget != null || isPromptVisible)
            {
                currentTarget = null;
                if (isPromptVisible) HidePrompt();
            }
        }
    }
    
    private bool IsValidInteractable(GameObject obj)
    {
        if (obj == null) return false;
        
        IClickable interactable = obj.GetComponent<IClickable>();
        if (interactable == null)
            interactable = obj.GetComponentInParent<IClickable>();
        
        if (interactable == null) return false;
        
        // Проверяем расстояние до игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return true;
        
        float distance = Vector3.Distance(obj.transform.position, player.transform.position);
        float range = interactable.GetInteractionRange();
        
        return distance <= range;
    }
    
    private void ShowPrompt()
    {
        if (userInterface != null)
        {
            userInterface.ShowHint(cachedPromptText);
            isPromptVisible = true;
            Debug.Log("InteractionPrompter: Показ подсказки");
        }
    }
    
    private void HidePrompt()
    {
        if (userInterface != null)
        {
            userInterface.HideHint();
            isPromptVisible = false;
            Debug.Log("InteractionPrompter: Скрытие подсказки");
        }
    }
    
    private string GetLocalizedText(string key)
    {
        return key;
    }
}