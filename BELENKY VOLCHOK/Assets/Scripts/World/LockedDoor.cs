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
    [SerializeField] private float timeLimit = 30f;
    [SerializeField] private float threatFadeStart = 15f;
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
    private Coroutine timeLimitCoroutine;
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
    private bool isUnlocked = true;
    
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
        if (player != null) playerTransform = player.transform;
    }
    
    private void OnDestroy()
    {
        if (knockMessageCoroutine != null)
            StopCoroutine(knockMessageCoroutine);
    }
    
    private void Update()
    {
        if (!isActive || waitingForAnswer || isDead) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) NextLine();
        if (isDoorOpen) ApplyDoorAnimation();
    }
    
    private void ApplyDoorAnimation()
    {
        doorPivot.localRotation = Quaternion.Slerp(doorPivot.localRotation, openRotation, Time.deltaTime * 5f);
    }
    
    public void OnInteract()
    {
        if (!isUnlocked) return;
        if (!IsPlayerInRange()) return;
        if (isActive || isDoorOpen) return;
        
        if (!hasShownKnockMessage)
        {
            hasShownKnockMessage = true;
            ShowKnockMessage();
        }
        
        StartMinigame();
    }
    
    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
    }
    
    private void ShowKnockMessage()
    {
        if (knockMessageCoroutine != null)
            StopCoroutine(knockMessageCoroutine);
        
        knockMessageCoroutine = StartCoroutine(ShowKnockMessageRoutine());
    }
    
    private IEnumerator ShowKnockMessageRoutine()
    {
        yield return new WaitForSeconds(knockMessageDelay);
        
        UserInterface ui = ServiceLocator.Get<UserInterface>();
        if (ui != null)
        {
            ui.ShowMessage(knockMessageKey, knockMessageDuration);
        }
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
        isActive = true;
        currentRiddleIndex = 0;
        if (cachedOutline != null) cachedOutline.enabled = false;
        FreezePlayer(true);
        if (minigamePanel != null) minigamePanel.SetActive(true);
        SetButtonsVisible(false);
        StartCoroutine(PlayKnockAndStart());
    }
    
    private IEnumerator PlayKnockAndStart()
    {
        AudioManager.instance?.Play(knockSound);
        yield return new WaitForSeconds(1f);
        StartRiddle();
    }
    
    private void StartRiddle()
    {
        if (currentRiddleIndex >= riddles.Count)
        {
            CompleteMinigame();
            return;
        }
        RiddleEntry entry = riddles[currentRiddleIndex];
        currentLineIndex = 0;
        waitingForAnswer = false;
        SetButtonsVisible(false);
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
        else ShowRiddle(entry);
    }
    
    private void ShowRiddle(RiddleEntry entry)
    {
        ShowTextLine(entry.riddleTextKey, () => {
            waitingForAnswer = true;
            SetButtonsVisible(true);
            SetButtonTexts(entry.leftAnswerKey, entry.rightAnswerKey);
            ThreatTimer.Instance?.StartThreatTimer(timeLimit, threatFadeStart, () => TimeoutKill());
        });
    }
    
    private void OnAnswerChosen(int answerIndex)
    {
        if (!waitingForAnswer) return;
        waitingForAnswer = false;
        SetButtonsVisible(false);
        ThreatTimer.Instance?.StopThreatTimer();
        RiddleEntry entry = riddles[currentRiddleIndex];
        bool isCorrect = (answerIndex == entry.correctAnswer);
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
        isActive = false;
        if (cachedOutline != null) cachedOutline.enabled = true;
        FreezePlayer(false);
        if (minigamePanel != null) minigamePanel.SetActive(false);
        
        FinalMonster finalMonster = FindObjectOfType<FinalMonster>(true);
        if (finalMonster != null)
        {
            finalMonster.gameObject.SetActive(true);
        }
    }
    
    private void TimeoutKill()
    {
        if (!waitingForAnswer) return;
        waitingForAnswer = false;
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
        PlayerMovement movement = FindObjectOfType<PlayerMovement>();
        if (movement != null) movement.SetMovementEnabled(!freeze);
        MouseLook mouseLook = FindObjectOfType<MouseLook>();
        if (mouseLook != null) mouseLook.enabled = !freeze;
        if (freeze) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        else { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    }
    
    private void SetButtonsVisible(bool visible)
    {
        if (answerButtonLeft != null) answerButtonLeft.gameObject.SetActive(visible);
        if (answerButtonRight != null) answerButtonRight.gameObject.SetActive(visible);
    }
    
    private void SetButtonTexts(string leftKey, string rightKey)
    {
        if (leftButtonText != null) leftButtonText.text = GetLocalizedText(leftKey);
        if (rightButtonText != null) rightButtonText.text = GetLocalizedText(rightKey);
    }
    
    private string GetLocalizedText(string key) { return key; }
}