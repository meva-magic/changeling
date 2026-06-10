using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Curtain : MonoBehaviour, IClickable
{
    [SerializeField] private GameObject openVisual;
    [SerializeField] private GameObject closedVisual;
    [SerializeField] private float requiredHoldTime = 3f;
    [SerializeField] private float fillSpeed = 1f;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private WindowMonsterPoint linkedWindow;
    [SerializeField] private GameObject progressPanel;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Image progressFill;
    [SerializeField] private string movementSound = "curtain_sound";
    
    private float holdProgress;
    private bool isHolding;
    private Transform playerTransform;
    private bool monsterDefeated;
    private Coroutine fillProcess;
    
    private void Start()
    {
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
    }
    
    private void Update()
    {
        bool stillHolding = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
        
        if (!stillHolding && isHolding)
        {
            CancelHold();
        }
    }
    
    public void OnInteract()
    {
        if (!isHolding)
        {
            BeginHold();
        }
    }
    
    private void BeginHold()
    {
        if (isHolding) return;
        
        isHolding = true;
        PlaySound();
        
        bool hasMonster = linkedWindow != null && linkedWindow.HasActiveMonster;
        
        if (hasMonster && !monsterDefeated)
        {
            if (progressPanel != null) progressPanel.SetActive(true);
            
            if (fillProcess != null) StopCoroutine(fillProcess);
            fillProcess = StartCoroutine(FillProcess());
        }
        
        SetClosedState();
    }
    
    private IEnumerator FillProcess()
    {
        float elapsed = holdProgress;
        
        while (isHolding)
        {
            bool stillHolding = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
            if (!stillHolding)
            {
                CancelHold();
                yield break;
            }
            
            if (!IsPlayerInRange())
            {
                CancelHold();
                yield break;
            }
            
            bool hasMonster = linkedWindow != null && linkedWindow.HasActiveMonster;
            if (!hasMonster)
            {
                CancelHold();
                yield break;
            }
            
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
        isHolding = false;
        
        if (fillProcess != null) StopCoroutine(fillProcess);
        
        holdProgress = 0;
        
        if (progressPanel != null) progressPanel.SetActive(false);
        UpdateProgress(0);
        SetOpenState();
        PlaySound();
        
        monsterDefeated = false;
    }
    
    private void DefeatMonster()
    {
        monsterDefeated = true;
        PlaySound();
        
        if (linkedWindow != null && linkedWindow.HasActiveMonster)
        {
            linkedWindow.BanishMonster();
        }
        
        if (progressPanel != null) progressPanel.SetActive(false);
        UpdateProgress(0);
    }
    
    private bool IsPlayerInRange()
    {
        if (playerTransform == null) return true;
        return Vector3.Distance(transform.position, playerTransform.position) <= interactionDistance;
    }
    
    private void UpdateProgress(float normalized)
    {
        if (progressSlider != null)
        {
            progressSlider.value = normalized * requiredHoldTime;
        }
        
        if (progressFill != null)
        {
            progressFill.fillAmount = normalized;
        }
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
        if (!string.IsNullOrEmpty(movementSound))
        {
            AudioManager.instance?.Play(movementSound);
        }
    }
    
    public string GetPromptKey()
    {
        return "";
    }
}