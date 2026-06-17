using UnityEngine;
using System.Collections;

public class FinalMonster : MonoBehaviour, IClickable
{
    [Header("Settings")]
    [SerializeField] private float interactionRange = 5f;
    [SerializeField] private GameObject outlineTarget;
    [SerializeField] private string dialogueKey = "final_monster_dialogue";
    [SerializeField] private string nextSceneName = "NextScene";
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float hoverDelay = 1.5f;
    
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 2f;
    [SerializeField] private float maxShakeMagnitude = 5f;
    [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private bool isActivated = false;
    private Outline cachedOutline;
    private Camera mainCamera;
    private float hoverTimer = 0f;
    private bool isHovering = false;
    private bool dialogueStarted = false;
    private PlayerMovement playerMovement;
    private CharacterController characterController;
    private Rigidbody playerRigidbody;
    private bool isShaking = false;
    private Quaternion savedCameraRotation;
    private Vector3 savedCameraPosition;
    private Vector3 savedPlayerPosition;
    private Quaternion savedPlayerRotation;
    private bool isDialogueActive = false;
    
    private GameObject EffectiveOutlineTarget
    {
        get { return outlineTarget != null ? outlineTarget : gameObject; }
    }
    
    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            savedCameraRotation = mainCamera.transform.localRotation;
            savedCameraPosition = mainCamera.transform.localPosition;
        }
        
        if (shakeCurve == null || shakeCurve.keys.Length == 0)
        {
            shakeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
        
        cachedOutline = EffectiveOutlineTarget.GetComponent<Outline>();
        if (cachedOutline != null)
            cachedOutline.enabled = false;
        
        playerMovement = FindObjectOfType<PlayerMovement>();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            characterController = player.GetComponent<CharacterController>();
            playerRigidbody = player.GetComponent<Rigidbody>();
            savedPlayerPosition = player.transform.position;
            savedPlayerRotation = player.transform.rotation;
        }
    }
    
    private void LateUpdate()
    {
        // Если диалог активен — фиксируем камеру И игрока
        if (isDialogueActive)
        {
            // Фиксируем камеру
            if (mainCamera != null)
            {
                mainCamera.transform.localPosition = savedCameraPosition;
                mainCamera.transform.localRotation = savedCameraRotation;
            }
            
            // Фиксируем игрока
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = savedPlayerPosition;
                player.transform.rotation = savedPlayerRotation;
            }
        }
    }
    
    private void Update()
    {
        if (isShaking)
        {
            return;
        }
        
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
        isDialogueActive = true;
        
        Debug.Log("FinalMonster: Начинаем диалог!");
        
        if (cachedOutline != null) cachedOutline.enabled = false;
        
        // Сохраняем текущее положение
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            savedPlayerPosition = player.transform.position;
            savedPlayerRotation = player.transform.rotation;
        }
        
        if (mainCamera != null)
        {
            savedCameraPosition = mainCamera.transform.localPosition;
            savedCameraRotation = mainCamera.transform.localRotation;
        }
        
        // Отключаем CharacterController
        if (characterController != null)
        {
            characterController.enabled = false;
            Debug.Log("FinalMonster: CharacterController отключен");
        }
        
        // Отключаем Rigidbody
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = true;
            playerRigidbody.velocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            Debug.Log("FinalMonster: Rigidbody заморожен");
        }
        
        // Блокируем движение игрока
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
        isDialogueActive = false;
        Debug.Log("FinalMonster: Диалог завершён, запускаем шейк и затемнение");
        StartCoroutine(ShakeAndFadeRoutine());
    }
    
    private IEnumerator ShakeAndFadeRoutine()
    {
        isShaking = true;
        
        float elapsed = 0f;
        float currentMagnitude = 0f;
        
        FadeToBlack.Instance?.FadeOut(null);
        
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;
            
            float curveValue = shakeCurve.Evaluate(progress);
            currentMagnitude = curveValue * maxShakeMagnitude;
            
            if (mainCamera != null)
            {
                float shakeX = Random.Range(-currentMagnitude, currentMagnitude);
                float shakeY = Random.Range(-currentMagnitude, currentMagnitude);
                float shakeZ = Random.Range(-currentMagnitude * 0.5f, currentMagnitude * 0.5f);
                mainCamera.transform.localRotation = savedCameraRotation * Quaternion.Euler(shakeX, shakeY, shakeZ);
            }
            
            yield return null;
        }
        
        if (mainCamera != null)
            mainCamera.transform.localRotation = savedCameraRotation;
        
        isShaking = false;
        
        Debug.Log($"FinalMonster: Загрузка сцены {nextSceneName}");
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
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