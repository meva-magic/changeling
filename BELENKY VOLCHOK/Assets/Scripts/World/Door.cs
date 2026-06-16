using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DoorRiddleMinigame : MonoBehaviour, IClickable
{
    [Header("UI References")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button answerButtonLeft;
    [SerializeField] private Button answerButtonRight;
    [SerializeField] private TextMeshProUGUI leftButtonText;
    [SerializeField] private TextMeshProUGUI rightButtonText;
    
    [Header("Visual Effects")]
    [SerializeField] private Transform monsterAppearancePoint;
    [SerializeField] private GameObject monsterUIPrefab;
    
    [Header("Door")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float openAngle = 90f;
    
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
    
    private Outline cachedOutline;
    private bool isActive = false;
    private int currentRiddleIndex = 0;
    private int currentLineIndex = 0;
    private bool waitingForAnswer = false;
    private Coroutine typingCoroutine;
    private string currentFullText;
    private GameObject spawnedMonster;
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
    private bool isWaitingForInput = false;
    private PlayerMovement playerMovement;
    private MouseLook mouseLook;
    private CharacterController characterController;
    private Vector3 savedPosition;
    private Quaternion savedRotation;
    private bool isFrozen = false;
    
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
        public List<string> preLines;
        public List<string> postLines;
    }
    
    private void Start()
    {
        cachedOutline = EffectiveOutlineTarget.GetComponent<Outline>();
        if (minigamePanel != null) minigamePanel.SetActive(false);
        if (answerButtonLeft != null) answerButtonLeft.onClick.AddListener(() => OnAnswerChosen(0));
        if (answerButtonRight != null) answerButtonRight.onClick.AddListener(() => OnAnswerChosen(1));
        closedRotation = doorPivot.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerMovement = player.GetComponent<PlayerMovement>();
            mouseLook = player.GetComponent<MouseLook>();
            characterController = player.GetComponent<CharacterController>();
        }
    }
    
    private void Update()
    {
        if (!isActive || waitingForAnswer || isDead) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) NextLine();
        if (isDoorOpen) ApplyDoorAnimation();
        
        if (isFrozen && playerTransform != null)
        {
            playerTransform.position = savedPosition;
            playerTransform.rotation = savedRotation;
        }
    }
    
    private void ApplyDoorAnimation()
    {
        doorPivot.localRotation = Quaternion.Slerp(doorPivot.localRotation, openRotation, Time.deltaTime * 5f);
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
        
        Debug.Log("DoorRiddleMinigame: OnInteract -> StartMinigame");
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
    
    public bool IsUnlocked()
    {
        return isUnlocked;
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
        Debug.Log("DoorRiddleMinigame: StartMinigame");
        isActive = true;
        isMinigameStarted = true;
        currentRiddleIndex = 0;
        
        if (cachedOutline != null) cachedOutline.enabled = false;
        
        FreezePlayer(true);
        
        if (minigamePanel != null) minigamePanel.SetActive(true);
        SetButtonsVisible(false);
        
        ThreatSystem.Instance?.StopCounterPermanently();
        
        Debug.Log("DoorRiddleMinigame: Счётчик угрозы полностью остановлен");
        
        StartRiddle();
    }
    
    private void StartRiddle()
    {
        Debug.Log($"DoorRiddleMinigame: StartRiddle, riddles.Count={riddles.Count}, currentRiddleIndex={currentRiddleIndex}");
        
        if (currentRiddleIndex >= riddles.Count)
        {
            Debug.Log("DoorRiddleMinigame: Все загадки пройдены -> CompleteMinigame");
            CompleteMinigame();
            return;
        }
        RiddleEntry entry = riddles[currentRiddleIndex];
        currentLineIndex = 0;
        waitingForAnswer = false;
        SetButtonsVisible(false);
        
        Debug.Log($"DoorRiddleMinigame: Обработка загадки {currentRiddleIndex}, preLines.Count={entry.preLines?.Count}");
        
        if (entry.preLines != null && entry.preLines.Count > 0)
        {
            ShowTextLine(entry.preLines[0], () => {
                currentLineIndex++;
                if (currentLineIndex < entry.preLines.Count)
                    ShowTextLine(entry.preLines[currentLineIndex], null);
                else
                    ShowRiddle(entry);
            });
        }
        else
        {
            Debug.Log("DoorRiddleMinigame: Нет preLines, сразу показываем загадку");
            ShowRiddle(entry);
        }
    }
    
    private void ShowRiddle(RiddleEntry entry)
    {
        Debug.Log($"DoorRiddleMinigame: ShowRiddle, riddleTextKey={entry.riddleTextKey}");
        
        ShowTextLine(entry.riddleTextKey, () => {
            waitingForAnswer = true;
            isWaitingForInput = true;
            SetButtonsVisible(true);
            SetButtonTexts(entry.leftAnswerKey, entry.rightAnswerKey);
            Debug.Log($"DoorRiddleMinigame: Кнопки показаны. Left: {entry.leftAnswerKey}, Right: {entry.rightAnswerKey}");
        });
    }
    
    private void OnAnswerChosen(int answerIndex)
    {
        Debug.Log($"DoorRiddleMinigame: OnAnswerChosen, answerIndex={answerIndex}");
        
        if (!waitingForAnswer) return;
        waitingForAnswer = false;
        isWaitingForInput = false;
        SetButtonsVisible(false);
        RiddleEntry entry = riddles[currentRiddleIndex];
        bool isCorrect = (answerIndex == entry.correctAnswer);
        
        Debug.Log($"DoorRiddleMinigame: Ответ {(isCorrect ? "ПРАВИЛЬНЫЙ" : "НЕПРАВИЛЬНЫЙ")}");
        
        if (isCorrect)
        {
            if (entry.postLines != null && entry.postLines.Count > 0)
            {
                currentLineIndex = 0;
                ShowTextLine(entry.postLines[0], () => {
                    currentLineIndex++;
                    if (currentLineIndex < entry.postLines.Count) ContinuePostLines();
                    else { currentRiddleIndex++; StartRiddle(); }
                });
            }
            else { currentRiddleIndex++; StartRiddle(); }
        }
        else
        {
            ShowTextLine("wrong_answer_dialogue", () => { StartCoroutine(OpenDoorAndKill()); });
        }
    }
    
    private void ContinuePostLines()
    {
        RiddleEntry entry = riddles[currentRiddleIndex];
        if (currentLineIndex < entry.postLines.Count)
            ShowTextLine(entry.postLines[currentLineIndex], () => {
                currentLineIndex++;
                ContinuePostLines();
            });
        else { currentRiddleIndex++; StartRiddle(); }
    }
    
    private void ShowTextLine(string textKey, System.Action onComplete)
    {
        currentFullText = GetLocalizedText(textKey);
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(currentFullText, onComplete));
    }
    
    private IEnumerator TypeText(string text, System.Action onComplete)
    {
        isDead = false;
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.05f);
        }
        onComplete?.Invoke();
    }
    
    private void NextLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentFullText;
        }
    }
    
    private void CompleteMinigame()
    {
        Debug.Log("DoorRiddleMinigame: CompleteMinigame");
        isActive = false;
        isWaitingForInput = false;
        if (cachedOutline != null) cachedOutline.enabled = true;
        FreezePlayer(false);
        if (minigamePanel != null) minigamePanel.SetActive(false);
        
        FinalMonster finalMonster = FindObjectOfType<FinalMonster>(true);
        if (finalMonster != null)
        {
            finalMonster.gameObject.SetActive(true);
            Debug.Log("DoorRiddleMinigame: Финальный монстр активирован");
        }
    }
    
    private void TimeoutKill()
    {
        if (!waitingForAnswer) return;
        waitingForAnswer = false;
        isWaitingForInput = false;
        SetButtonsVisible(false);
        ShowTextLine("timeout_dialogue", () => { StartCoroutine(SpawnMonsterAndKill()); });
    }
    
    private IEnumerator SpawnMonsterAndKill()
    {
        if (monsterUIPrefab != null && monsterAppearancePoint != null)
            spawnedMonster = Instantiate(monsterUIPrefab, monsterAppearancePoint.position, Quaternion.identity);
        yield return new WaitForSeconds(1f);
        KillPlayer();
    }
    
    private IEnumerator OpenDoorAndKill()
    {
        AudioManager.instance?.Play(doorOpenSound);
        isDoorOpen = true;
        yield return new WaitForSeconds(1f);
        KillPlayer();
    }
    
    private void KillPlayer()
    {
        if (isDead) return;
        isDead = true;
        if (cachedOutline != null) cachedOutline.enabled = true;
        FreezePlayer(false);
        if (minigamePanel != null) minigamePanel.SetActive(false);
        PenaltySystem.Instance?.TriggerDeath();
    }
    
    private void FreezePlayer(bool freeze)
    {
        if (freeze)
        {
            if (playerTransform != null)
            {
                savedPosition = playerTransform.position;
                savedRotation = playerTransform.rotation;
                isFrozen = true;
            }
            
            if (playerMovement != null)
                playerMovement.SetMovementEnabled(false);
            
            if (mouseLook != null)
                mouseLook.enabled = false;
            
            if (characterController != null)
                characterController.enabled = false;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            isFrozen = false;
            
            if (playerMovement != null)
                playerMovement.SetMovementEnabled(true);
            
            if (mouseLook != null)
                mouseLook.enabled = true;
            
            if (characterController != null)
                characterController.enabled = true;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    private void SetButtonsVisible(bool visible)
    {
        if (answerButtonLeft != null) answerButtonLeft.gameObject.SetActive(visible);
        if (answerButtonRight != null) answerButtonRight.gameObject.SetActive(visible);
        Debug.Log($"DoorRiddleMinigame: Кнопки видимы = {visible}");
    }
    
    private void SetButtonTexts(string leftKey, string rightKey)
    {
        if (leftButtonText != null) leftButtonText.text = GetLocalizedText(leftKey);
        if (rightButtonText != null) rightButtonText.text = GetLocalizedText(rightKey);
    }
    
    private string GetLocalizedText(string key) { return key; }
}