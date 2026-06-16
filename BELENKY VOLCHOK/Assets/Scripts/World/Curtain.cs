using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CurtainController : MonoBehaviour, IClickable
{
    [Header("Visuals")]
    [SerializeField] private GameObject openVisual;
    [SerializeField] private GameObject closedVisual;
    
    [Header("Target Object for Outline")]
    [SerializeField] private GameObject outlineTarget;
    
    [Header("Settings")]
    [SerializeField] private float requiredHoldTime = 3f;
    [SerializeField] private float fillSpeed = 1f;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private WindowMonsterPoint linkedWindow;
    [SerializeField] private GameObject progressPanel;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Image progressFill;
    [SerializeField] private string movementSound = "curtain_sound";
    [SerializeField] private float threatTime = 20f;
    [SerializeField] private float threatFadeStart = 10f;
    
    private float holdProgress;
    private bool isHolding;
    private Transform playerTransform;
    private bool monsterDefeated;
    private Coroutine fillProcess;
    private bool isTimerActive;
    private Outline cachedOutline;
    private bool wasOutlineEnabled;
    
    private GameObject EffectiveOutlineTarget
    {
        get { return outlineTarget != null ? outlineTarget : gameObject; }
    }
    
    private void Start()
    {
        cachedOutline = EffectiveOutlineTarget.GetComponent<Outline>();
        if (cachedOutline != null)
        {
            wasOutlineEnabled = cachedOutline.enabled;
        }
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
        SetOpenState();
        if (progressPanel != null) progressPanel.SetActive(false);
        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = requiredHoldTime;
            progressSlider.value = 0;
        }
        Debug.Log($"CurtainController {gameObject.name}: Инициализирован");
    }
    
    private void Update()
    {
        bool stillHolding = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
        if (!stillHolding && isHolding) CancelHold();
    }
    
    public void OnInteract()
    {
        Debug.Log($"CurtainController {gameObject.name}: OnInteract вызван");
        
        if (!IsPlayerInRange())
        {
            Debug.Log($"CurtainController {gameObject.name}: Игрок слишком далеко");
            return;
        }
        
        if (!isHolding)
        {
            Debug.Log($"CurtainController {gameObject.name}: Начинаем удержание");
            BeginHold();
        }
        else
        {
            Debug.Log($"CurtainController {gameObject.name}: Уже удерживается");
        }
    }
    
    private bool IsPlayerInRange()
    {
        if (playerTransform == null) return true;
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        Debug.Log($"CurtainController {gameObject.name}: Расстояние до игрока = {distance}, требуется <= {interactionDistance}");
        return distance <= interactionDistance;
    }
    
    public float GetInteractionRange()
    {
        return interactionDistance;
    }
    
    public string GetPromptKey()
    {
        return "";
    }
    
    public GameObject GetOutlineTarget()
    {
        return EffectiveOutlineTarget;
    }
    
    public void RestoreOutline()
    {
        if (cachedOutline != null)
        {
            cachedOutline.enabled = wasOutlineEnabled;
        }
    }
    
    private void BeginHold()
    {
        if (isHolding) return;
        isHolding = true;
        PlaySound();
        bool hasMonster = linkedWindow != null && linkedWindow.HasActiveMonster;
        
        Debug.Log($"CurtainController {gameObject.name}: BeginHold, hasMonster={hasMonster}, monsterDefeated={monsterDefeated}");
        
        if (hasMonster && !monsterDefeated)
        {
            if (cachedOutline != null)
            {
                wasOutlineEnabled = cachedOutline.enabled;
                cachedOutline.enabled = false;
            }
            if (progressPanel != null) progressPanel.SetActive(true);
            if (fillProcess != null) StopCoroutine(fillProcess);
            fillProcess = StartCoroutine(FillProcess());
            if (!isTimerActive)
            {
                isTimerActive = true;
                ThreatTimer.Instance?.StartThreatTimer(threatTime, threatFadeStart, () => KillPlayer());
            }
        }
        SetClosedState();
    }
    
    private IEnumerator FillProcess()
    {
        float elapsed = holdProgress;
        Debug.Log($"CurtainController {gameObject.name}: FillProcess начат");
        
        while (isHolding)
        {
            bool stillHolding = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
            if (!stillHolding) { CancelHold(); yield break; }
            if (!IsPlayerInRange()) { CancelHold(); yield break; }
            bool hasMonster = linkedWindow != null && linkedWindow.HasActiveMonster;
            if (!hasMonster) { CancelHold(); yield break; }
            elapsed += fillSpeed * Time.deltaTime;
            holdProgress = Mathf.Min(elapsed, requiredHoldTime);
            UpdateProgress(holdProgress / requiredHoldTime);
            if (elapsed >= requiredHoldTime)
            {
                DefeatMonster();
                yield break;
            }
            yield return null;
        }
    }
    
    private void CancelHold()
    {
        Debug.Log($"CurtainController {gameObject.name}: CancelHold");
        isHolding = false;
        if (fillProcess != null) StopCoroutine(fillProcess);
        holdProgress = 0;
        if (progressPanel != null) progressPanel.SetActive(false);
        UpdateProgress(0);
        SetOpenState();
        PlaySound();
        monsterDefeated = false;
        RestoreOutline();
        if (isTimerActive)
        {
            ThreatTimer.Instance?.StopThreatTimer();
            isTimerActive = false;
        }
    }
    
    private void DefeatMonster()
    {
        Debug.Log($"CurtainController {gameObject.name}: DefeatMonster");
        monsterDefeated = true;
        PlaySound();
        if (linkedWindow != null && linkedWindow.HasActiveMonster) linkedWindow.BanishMonster();
        if (progressPanel != null) progressPanel.SetActive(false);
        UpdateProgress(0);
        RestoreOutline();
        if (isTimerActive)
        {
            ThreatTimer.Instance?.StopThreatTimer();
            isTimerActive = false;
        }
    }
    
    private void KillPlayer()
    {
        PenaltySystem.Instance?.TriggerDeath();
    }
    
    private void UpdateProgress(float normalized)
    {
        if (progressSlider != null) progressSlider.value = normalized * requiredHoldTime;
        if (progressFill != null) progressFill.fillAmount = normalized;
    }
    
    private void SetOpenState()
    {
        if (openVisual != null) openVisual.SetActive(true);
        if (closedVisual != null) closedVisual.SetActive(false);
    }
    
    private void SetClosedState()
    {
        if (openVisual != null) openVisual.SetActive(false);
        if (closedVisual != null) closedVisual.SetActive(true);
    }
    
    private void PlaySound()
    {
        if (!string.IsNullOrEmpty(movementSound)) AudioManager.instance?.Play(movementSound);
    }
}