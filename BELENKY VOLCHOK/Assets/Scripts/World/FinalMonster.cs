using UnityEngine;

public class FinalMonster : MonoBehaviour, IClickable
{
    [Header("Settings")]
    [SerializeField] private float interactionRange = 5f;
    [SerializeField] private GameObject outlineTarget;
    [SerializeField] private string dialogueKey = "final_monster_dialogue";
    [SerializeField] private string nextSceneName = "NextScene";
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float hoverDelay = 1.5f;
    
    private bool isActivated = false;
    private Outline cachedOutline;
    private Camera mainCamera;
    private float hoverTimer = 0f;
    private bool isHovering = false;
    private bool dialogueStarted = false;
    private PlayerMovement playerMovement;
    
    private GameObject EffectiveOutlineTarget
    {
        get { return outlineTarget != null ? outlineTarget : gameObject; }
    }
    
    private void Start()
    {
        mainCamera = Camera.main;
        cachedOutline = EffectiveOutlineTarget.GetComponent<Outline>();
        if (cachedOutline != null)
            cachedOutline.enabled = false;
        
        playerMovement = FindObjectOfType<PlayerMovement>();
    }
    
    private void Update()
    {
        if (!isActivated && !dialogueStarted)
        {
            bool isLooking = IsPlayerLookingAtMonster();
            
            if (isLooking && IsPlayerInRange())
            {
                if (!isHovering)
                {
                    isHovering = true;
                    hoverTimer = 0f;
                }
                
                hoverTimer += Time.deltaTime;
                
                if (hoverTimer >= hoverDelay)
                {
                    StartDialogue();
                }
            }
            else
            {
                if (isHovering)
                {
                    isHovering = false;
                    hoverTimer = 0f;
                }
            }
        }
    }
    
    private bool IsPlayerLookingAtMonster()
    {
        if (mainCamera == null) return false;
        
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            return hit.collider.gameObject == gameObject;
        }
        return false;
    }
    
    private void StartDialogue()
    {
        if (dialogueStarted) return;
        dialogueStarted = true;
        isActivated = true;
        
        Debug.Log("FinalMonster: Начинаем диалог!");
        
        if (cachedOutline != null) cachedOutline.enabled = false;
        
        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(false);
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        DialogueSystem.Instance.SetBlockPlayerInput(true);
        DialogueSystem.Instance.SetAutoCloseDelay(0);
        DialogueSystem.Instance.ShowDialogue(dialogueKey, OnDialogueComplete, false);
    }
    
    private void OnDialogueComplete()
    {
        Debug.Log("FinalMonster: Диалог завершён, запускаем затемнение");
        
        if (FadeToBlack.Instance != null)
        {
            Debug.Log("FinalMonster: FadeToBlack найден, запускаем анимацию");
            FadeToBlack.Instance.FadeOut(() => {
                Debug.Log($"FinalMonster: Загрузка сцены {nextSceneName}");
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            });
        }
        else
        {
            Debug.LogError("FinalMonster: FadeToBlack.Instance не найден! Загружаем сцену без затемнения.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
    
    private bool IsPlayerInRange()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return true;
        return Vector3.Distance(transform.position, player.transform.position) <= interactionRange;
    }
    
    public void OnInteract() { }
    public string GetPromptKey() { return ""; }
    public float GetInteractionRange() { return interactionRange; }
    public GameObject GetOutlineTarget() { return EffectiveOutlineTarget; }
}