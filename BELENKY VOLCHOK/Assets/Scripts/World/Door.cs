using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DoorRiddleMinigame : MonoBehaviour, IClickable
{
    [Header("UI Settings")]
    [SerializeField] private float panelWidth = 1200f;
    [SerializeField] private float panelHeight = 800f;
    [SerializeField] private float fontSize = 80f;
    [SerializeField] private float buttonFontSize = 60f;
    [SerializeField] private TMP_FontAsset fontAsset;
    [SerializeField] private Color panelColor = new Color(0, 0, 0, 0.85f);
    [SerializeField] private Color buttonTintColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color buttonHoverTintColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color textColor = Color.white;
    
    [Header("Sprites (Optional)")]
    [SerializeField] private Sprite panelBackgroundSprite;
    [SerializeField] private Sprite buttonSprite;
    
    [Header("Door")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float doorOpenSpeed = 3f;
    
    [Header("Audio")]
    [SerializeField] private string knockSound = "door_knock";
    [SerializeField] private string doorOpenSound = "door_open";
    
    [Header("Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private GameObject outlineTarget;
    [SerializeField] private float knockMessageDelay = 5f;
    [SerializeField] private string knockMessageKey = "door_knock_message";
    [SerializeField] private float knockMessageDuration = 3f;
    
    [Header("Riddles")]
    [SerializeField] private List<RiddleEntry> riddles = new List<RiddleEntry>();
    
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 1.5f;
    [SerializeField] private float maxShakeMagnitude = 8f;
    [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private Outline cachedOutline;
    private bool isActive = false;
    private int currentRiddleIndex = 0;
    private bool waitingForAnswer = false;
    private Coroutine typingCoroutine;
    private string currentFullText;
    private bool isDead = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isDoorOpen = false;
    private Transform playerTransform;
    private bool hasShownKnockMessage = false;
    private Coroutine knockMessageCoroutine;
    private bool isUnlocked = false;
    private bool isMinigameStarted = false;
    private bool knockTimerStarted = false;
    private PlayerMovement playerMovement;
    private CharacterController characterController;
    private Rigidbody playerRigidbody;
    private MouseLook mouseLook;
    
    private Vector3 savedPlayerPosition;
    private Quaternion savedPlayerRotation;
    private bool isFrozen = false;
    private bool isDeathTriggered = false;
    private bool isDoorOpening = false;
    private bool isDialogueShowing = false;
    private bool isTyping = false;
    private System.Action dialogueCompleteCallback;
    private bool isShaking = false;
    
    private GameObject customCanvas;
    private GameObject customPanel;
    private TextMeshProUGUI customDialogueText;
    private Button customLeftBtn;
    private Button customRightBtn;
    private TextMeshProUGUI customLeftText;
    private TextMeshProUGUI customRightText;
    
    private Camera mainCamera;
    private Quaternion originalCameraRotation;
    private Vector3 originalCameraPosition;
    
    private bool isThreatActive = false;
    private bool isThreatComplete = false;
    
    private GameObject EffectiveOutlineTarget
    {
        get { return outlineTarget != null ? outlineTarget : gameObject; }
    }
    
    [System.Serializable]
    public class RiddleEntry
    {
        public string riddleTextKey;
        public string leftAnswerKey;
        public string rightAnswerKey;
        public int correctAnswer;
        public string wrongAnswerDialogueKey = "wrong_answer_dialogue";
    }
    
    private void Start()
    {
        Debug.Log("=== DoorRiddleMinigame: Start ===");
        
        cachedOutline = EffectiveOutlineTarget.GetComponent<Outline>();
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalCameraRotation = mainCamera.transform.localRotation;
            originalCameraPosition = mainCamera.transform.localPosition;
        }
        
        if (shakeCurve == null || shakeCurve.keys.Length == 0)
        {
            shakeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
        
        closedRotation = doorPivot.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerMovement = player.GetComponent<PlayerMovement>();
            characterController = player.GetComponent<CharacterController>();
            playerRigidbody = player.GetComponent<Rigidbody>();
            mouseLook = player.GetComponent<MouseLook>();
            savedPlayerPosition = playerTransform.position;
            savedPlayerRotation = playerTransform.rotation;
        }
    }
    
    private void LateUpdate()
    {
        if (isFrozen && playerTransform != null)
        {
            playerTransform.position = savedPlayerPosition;
            playerTransform.rotation = savedPlayerRotation;
        }
    }
    
    private void Update()
    {
        if (isThreatActive && !isThreatComplete)
        {
            float progress = ThreatSystem.Instance?.GetProgress() ?? 0f;
            if (progress >= 99f)
            {
                isThreatComplete = true;
                Debug.Log("DoorRiddleMinigame: Угроза достигла 100%! Вызываем смерть");
                StartCoroutine(HandleThreatDeath());
            }
        }
        
        if (isDialogueShowing && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            if (!isTyping)
            {
                CloseDialogue();
            }
            else if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                customDialogueText.text = currentFullText;
                isTyping = false;
                CloseDialogue();
            }
        }
        
        if (isDoorOpening)
        {
            ApplyDoorAnimation();
            return;
        }
        
        if (isShaking) return;
        
        if (isFrozen && playerTransform != null)
        {
            playerTransform.position = savedPlayerPosition;
            playerTransform.rotation = savedPlayerRotation;
        }
        
        if (!isActive || waitingForAnswer || isDead) return;
        if (isDoorOpen) ApplyDoorAnimation();
    }
    
    private IEnumerator HandleThreatDeath()
    {
        isShaking = true;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            savedPlayerPosition = player.transform.position;
            savedPlayerRotation = player.transform.rotation;
        }
        
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = true;
            playerRigidbody.velocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }
        
        if (mouseLook != null)
        {
            mouseLook.enabled = false;
        }
        
        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.localPosition;
            originalCameraRotation = mainCamera.transform.localRotation;
        }
        
        float elapsed = 0f;
        float currentMagnitude = 0f;
        float duration = 1f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            currentMagnitude = progress * 5f;
            
            if (mainCamera != null)
            {
                float shakeX = Random.Range(-currentMagnitude, currentMagnitude);
                float shakeY = Random.Range(-currentMagnitude, currentMagnitude);
                float shakeZ = Random.Range(-currentMagnitude * 0.5f, currentMagnitude * 0.5f);
                mainCamera.transform.localRotation = originalCameraRotation * Quaternion.Euler(shakeX, shakeY, shakeZ);
            }
            
            if (player != null)
            {
                player.transform.position = savedPlayerPosition;
                player.transform.rotation = savedPlayerRotation;
            }
            
            yield return null;
        }
        
        if (mainCamera != null)
            mainCamera.transform.localRotation = originalCameraRotation;
        
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
        }
        
        if (mouseLook != null)
        {
            mouseLook.enabled = true;
        }
        
        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(false);
        }
        
        if (customCanvas != null)
            Destroy(customCanvas);
        
        PenaltySystem.Instance?.TriggerDeath();
    }
    
    private void CloseDialogue()
    {
        if (!isDialogueShowing) return;
        isDialogueShowing = false;
        isTyping = false;
        
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        
        Debug.Log("DoorRiddleMinigame: Диалог закрыт");
        
        if (dialogueCompleteCallback != null)
        {
            var callback = dialogueCompleteCallback;
            dialogueCompleteCallback = null;
            callback?.Invoke();
        }
    }
    
    private void ApplyDoorAnimation()
    {
        doorPivot.localRotation = Quaternion.Slerp(doorPivot.localRotation, openRotation, Time.deltaTime * doorOpenSpeed);
        
        if (Quaternion.Angle(doorPivot.localRotation, openRotation) < 0.5f)
        {
            doorPivot.localRotation = openRotation;
            isDoorOpen = true;
            isDoorOpening = false;
            Debug.Log("DoorRiddleMinigame: Дверь полностью открыта");
        }
    }
    
    public void OnInteract()
    {
        if (!isUnlocked)
        {
            Debug.Log("DoorRiddleMinigame: Дверь ещё не разблокирована");
            return;
        }
        
        if (!IsPlayerInRange()) return;
        if (isActive || isDoorOpen) return;
        
        if (knockMessageCoroutine != null)
        {
            StopCoroutine(knockMessageCoroutine);
            knockMessageCoroutine = null;
            UserInterface ui = ServiceLocator.Get<UserInterface>();
            if (ui != null) ui.HideMessage();
        }
        
        StartMinigame();
    }
    
    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
        Debug.Log($"DoorRiddleMinigame: Дверь разблокирована = {unlocked}");
        
        if (isUnlocked && !knockTimerStarted)
        {
            knockTimerStarted = true;
            AudioManager.instance?.Play(knockSound);
            knockMessageCoroutine = StartCoroutine(KnockMessageTimer());
        }
        
        if (isUnlocked && cachedOutline != null)
        {
            cachedOutline.enabled = true;
        }
    }
    
    public bool IsActive()
    {
        return isActive;
    }
    
    private IEnumerator KnockMessageTimer()
    {
        yield return new WaitForSeconds(knockMessageDelay);
        
        if (!isActive && !isMinigameStarted)
        {
            UserInterface ui = ServiceLocator.Get<UserInterface>();
            if (ui != null)
            {
                ui.ShowMessage(knockMessageKey, knockMessageDuration);
                hasShownKnockMessage = true;
            }
        }
        knockMessageCoroutine = null;
    }
    
    private bool IsPlayerInRange()
    {
        if (playerTransform == null) return true;
        return Vector3.Distance(transform.position, playerTransform.position) <= interactionRange;
    }
    
    public float GetInteractionRange()
    {
        return interactionRange;
    }
    
    public string GetPromptKey()
    {
        return "locked_door";
    }
    
    public GameObject GetOutlineTarget()
    {
        return EffectiveOutlineTarget;
    }
    
    private void StartMinigame()
    {
        Debug.Log("=== DoorRiddleMinigame: StartMinigame ===");
        
        if (playerTransform != null)
        {
            savedPlayerPosition = playerTransform.position;
            savedPlayerRotation = playerTransform.rotation;
            isFrozen = true;
        }
        
        isActive = true;
        isMinigameStarted = true;
        currentRiddleIndex = 0;
        isDeathTriggered = false;
        isDoorOpening = false;
        isDialogueShowing = false;
        isTyping = false;
        isShaking = false;
        isThreatActive = true;
        isThreatComplete = false;
        
        if (cachedOutline != null) cachedOutline.enabled = false;
        
        FreezePlayer(true);
        CreateCustomUI();
        ThreatSystem.Instance?.ResetAll();
        
        StartRiddle();
    }
    
    private void CreateCustomUI()
    {
        customCanvas = new GameObject("CustomMinigameCanvas");
        Canvas canvas = customCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        customCanvas.AddComponent<GraphicRaycaster>();
        
        customPanel = new GameObject("CustomPanel");
        customPanel.transform.SetParent(customCanvas.transform);
        
        RectTransform panelRect = customPanel.AddComponent<RectTransform>();
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        
        Image panelImg = customPanel.AddComponent<Image>();
        if (panelBackgroundSprite != null)
        {
            panelImg.sprite = panelBackgroundSprite;
            panelImg.color = Color.white;
            panelImg.type = Image.Type.Sliced;
        }
        else
        {
            panelImg.color = panelColor;
        }
        panelImg.raycastTarget = true;
        
        GameObject dialogueObj = new GameObject("CustomDialogue");
        dialogueObj.transform.SetParent(customPanel.transform);
        
        RectTransform dialogueRect = dialogueObj.AddComponent<RectTransform>();
        dialogueRect.anchoredPosition = new Vector2(0, 150);
        dialogueRect.sizeDelta = new Vector2(panelWidth - 100, 250);
        
        customDialogueText = dialogueObj.AddComponent<TextMeshProUGUI>();
        customDialogueText.fontSize = fontSize;
        customDialogueText.alignment = TextAlignmentOptions.Center;
        customDialogueText.color = textColor;
        customDialogueText.raycastTarget = false;
        customDialogueText.text = "";
        customDialogueText.fontStyle = FontStyles.Bold;
        
        if (fontAsset != null)
        {
            customDialogueText.font = fontAsset;
        }
        
        GameObject leftObj = new GameObject("CustomLeftBtn");
        leftObj.transform.SetParent(customPanel.transform);
        
        RectTransform leftRect = leftObj.AddComponent<RectTransform>();
        leftRect.anchoredPosition = new Vector2(-250, -150);
        leftRect.sizeDelta = new Vector2(350, 100);
        
        Image leftImg = leftObj.AddComponent<Image>();
        if (buttonSprite != null)
        {
            leftImg.sprite = buttonSprite;
            leftImg.type = Image.Type.Sliced;
        }
        leftImg.color = buttonTintColor;
        leftImg.raycastTarget = true;
        
        customLeftBtn = leftObj.AddComponent<Button>();
        customLeftBtn.targetGraphic = leftImg;
        customLeftBtn.onClick.AddListener(() => OnAnswerChosen(0));
        
        ColorBlock leftColors = new ColorBlock();
        leftColors.normalColor = buttonTintColor;
        leftColors.highlightedColor = buttonHoverTintColor;
        leftColors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        leftColors.selectedColor = buttonTintColor;
        leftColors.colorMultiplier = 1f;
        leftColors.fadeDuration = 0.1f;
        customLeftBtn.colors = leftColors;
        
        GameObject leftTextObj = new GameObject("LeftText");
        leftTextObj.transform.SetParent(leftObj.transform);
        customLeftText = leftTextObj.AddComponent<TextMeshProUGUI>();
        customLeftText.fontSize = buttonFontSize;
        customLeftText.alignment = TextAlignmentOptions.Center;
        customLeftText.color = textColor;
        customLeftText.raycastTarget = false;
        customLeftText.fontStyle = FontStyles.Bold;
        
        if (fontAsset != null)
        {
            customLeftText.font = fontAsset;
        }
        
        RectTransform leftTextRect = leftTextObj.GetComponent<RectTransform>();
        leftTextRect.anchoredPosition = Vector2.zero;
        leftTextRect.sizeDelta = new Vector2(350, 100);
        
        GameObject rightObj = new GameObject("CustomRightBtn");
        rightObj.transform.SetParent(customPanel.transform);
        
        RectTransform rightRect = rightObj.AddComponent<RectTransform>();
        rightRect.anchoredPosition = new Vector2(250, -150);
        rightRect.sizeDelta = new Vector2(350, 100);
        
        Image rightImg = rightObj.AddComponent<Image>();
        if (buttonSprite != null)
        {
            rightImg.sprite = buttonSprite;
            rightImg.type = Image.Type.Sliced;
        }
        rightImg.color = buttonTintColor;
        rightImg.raycastTarget = true;
        
        customRightBtn = rightObj.AddComponent<Button>();
        customRightBtn.targetGraphic = rightImg;
        customRightBtn.onClick.AddListener(() => OnAnswerChosen(1));
        
        ColorBlock rightColors = new ColorBlock();
        rightColors.normalColor = buttonTintColor;
        rightColors.highlightedColor = buttonHoverTintColor;
        rightColors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        rightColors.selectedColor = buttonTintColor;
        rightColors.colorMultiplier = 1f;
        rightColors.fadeDuration = 0.1f;
        customRightBtn.colors = rightColors;
        
        GameObject rightTextObj = new GameObject("RightText");
        rightTextObj.transform.SetParent(rightObj.transform);
        customRightText = rightTextObj.AddComponent<TextMeshProUGUI>();
        customRightText.fontSize = buttonFontSize;
        customRightText.alignment = TextAlignmentOptions.Center;
        customRightText.color = textColor;
        customRightText.raycastTarget = false;
        customRightText.fontStyle = FontStyles.Bold;
        
        if (fontAsset != null)
        {
            customRightText.font = fontAsset;
        }
        
        RectTransform rightTextRect = rightTextObj.GetComponent<RectTransform>();
        rightTextRect.anchoredPosition = Vector2.zero;
        rightTextRect.sizeDelta = new Vector2(350, 100);
        
        customLeftBtn.gameObject.SetActive(false);
        customRightBtn.gameObject.SetActive(false);
        
        Debug.Log("!!! CUSTOM UI CREATED !!!");
    }
    
    private void StartRiddle()
    {
        Debug.Log($"=== DoorRiddleMinigame: StartRiddle, riddles.Count={riddles.Count} ===");
        
        if (riddles.Count == 0)
        {
            Debug.LogError("DoorRiddleMinigame: НЕТ ЗАГАДОК!");
            CompleteMinigame();
            return;
        }
        
        if (currentRiddleIndex >= riddles.Count)
        {
            CompleteMinigame();
            return;
        }
        
        RiddleEntry entry = riddles[currentRiddleIndex];
        
        ShowTextWithButtons(entry.riddleTextKey, entry.leftAnswerKey, entry.rightAnswerKey, () => {
            waitingForAnswer = true;
            Debug.Log($"DoorRiddleMinigame: Кнопки активны. Left: {entry.leftAnswerKey}, Right: {entry.rightAnswerKey}");
        });
    }
    
    private void ShowTextWithButtons(string textKey, string leftKey, string rightKey, System.Action onComplete)
    {
        string text = GetLocalizedText(textKey);
        customDialogueText.text = text;
        customDialogueText.gameObject.SetActive(true);
        
        customLeftBtn.gameObject.SetActive(true);
        customRightBtn.gameObject.SetActive(true);
        customLeftText.text = GetLocalizedText(leftKey);
        customRightText.text = GetLocalizedText(rightKey);
        
        Debug.Log($"DoorRiddleMinigame: Текст показан: {text}");
        
        onComplete?.Invoke();
    }
    
    private void ShowWrongAnswerDialogue(string textKey, System.Action onComplete)
    {
        isDialogueShowing = true;
        isTyping = true;
        dialogueCompleteCallback = onComplete;
        string text = GetLocalizedText(textKey);
        currentFullText = text;
        
        customDialogueText.text = text;
        customDialogueText.gameObject.SetActive(true);
        customLeftBtn.gameObject.SetActive(false);
        customRightBtn.gameObject.SetActive(false);
        
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeCustomText(text));
    }
    
    private IEnumerator TypeCustomText(string text)
    {
        customDialogueText.text = "";
        foreach (char c in text)
        {
            customDialogueText.text += c;
            yield return new WaitForSeconds(0.05f);
        }
        isTyping = false;
        Debug.Log("DoorRiddleMinigame: Текст диалога показан: " + customDialogueText.text);
    }
    
    private void OnAnswerChosen(int answerIndex)
    {
        Debug.Log($"=== DoorRiddleMinigame: OnAnswerChosen вызван! answerIndex={answerIndex} ===");
        
        if (!waitingForAnswer)
        {
            Debug.Log("DoorRiddleMinigame: Не ожидаем ответа");
            return;
        }
        
        waitingForAnswer = false;
        customLeftBtn.gameObject.SetActive(false);
        customRightBtn.gameObject.SetActive(false);
        
        RiddleEntry entry = riddles[currentRiddleIndex];
        bool isCorrect = (answerIndex == entry.correctAnswer);
        
        if (isCorrect)
        {
            Debug.Log("Правильный ответ!");
            currentRiddleIndex++;
            StartRiddle();
        }
        else
        {
            Debug.Log("Неправильный ответ! Показываем диалог");
            ShowWrongAnswerDialogue(entry.wrongAnswerDialogueKey, () => {
                Debug.Log("Диалог закрыт, открываем дверь и убиваем игрока");
                StartCoroutine(OpenDoorAndKillSequence());
            });
        }
    }
    
    private IEnumerator OpenDoorAndKillSequence()
    {
        if (isDeathTriggered) yield break;
        isDeathTriggered = true;
        isDoorOpening = true;
        isShaking = true;
        
        if (customCanvas != null)
        {
            customCanvas.SetActive(false);
            Debug.Log("DoorRiddleMinigame: Мини-игра скрыта");
        }
        
        Debug.Log("DoorRiddleMinigame: Открываем дверь");
        AudioManager.instance?.Play(doorOpenSound);
        
        float timer = 0f;
        while (!isDoorOpen && timer < 3f)
        {
            timer += Time.deltaTime;
            doorPivot.localRotation = Quaternion.Slerp(doorPivot.localRotation, openRotation, Time.deltaTime * doorOpenSpeed * 2f);
            yield return null;
        }
        
        doorPivot.localRotation = openRotation;
        isDoorOpen = true;
        isDoorOpening = false;
        
        Debug.Log("DoorRiddleMinigame: Дверь открыта");
        
        PenaltySystem.Instance?.TriggerDeath();
    }
    
    private void CompleteMinigame()
    {
        Debug.Log("=== DoorRiddleMinigame: CompleteMinigame ===");
        isActive = false;
        isFrozen = false;
        if (cachedOutline != null) cachedOutline.enabled = true;
        FreezePlayer(false);
        
        if (customCanvas != null)
            Destroy(customCanvas);
        
        FinalMonster finalMonster = FindObjectOfType<FinalMonster>(true);
        if (finalMonster != null)
        {
            finalMonster.gameObject.SetActive(true);
        }
    }
    
    private void FreezePlayer(bool freeze)
    {
        if (freeze)
        {
            isFrozen = true;
            if (playerMovement != null) playerMovement.SetMovementEnabled(false);
            if (characterController != null) characterController.enabled = false;
            if (playerRigidbody != null)
            {
                playerRigidbody.isKinematic = true;
                playerRigidbody.velocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }
            if (mouseLook != null) mouseLook.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            isFrozen = false;
            if (playerMovement != null) playerMovement.SetMovementEnabled(true);
            if (characterController != null) characterController.enabled = true;
            if (playerRigidbody != null) playerRigidbody.isKinematic = false;
            if (mouseLook != null) mouseLook.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    private string GetLocalizedText(string key) { return key; }
}