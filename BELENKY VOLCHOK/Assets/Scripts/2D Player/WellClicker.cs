using UnityEngine;
using UnityEngine.UI;

public class ClickerMinigame : MonoBehaviour
{
    [SerializeField] private GameObject itemToActivate;
    [SerializeField] private float clickProgress = 2f;
    [SerializeField] private float decayPerSecond = 5f;
    [SerializeField] private GameObject minigameCanvas;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private PlayerMovement playerMovement;
    
    [Header("Bucket Animation")]
    [SerializeField] private Image bucketImage;
    [SerializeField] private RectTransform bucketStartPoint;
    [SerializeField] private RectTransform bucketEndPoint;
    [SerializeField] private AnimationCurve bucketCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Interaction Indicator")]
    [SerializeField] private GameObject interactionIndicator;
    
    private float currentProgress;
    private bool minigameActive;
    private bool isCompleted;
    private bool playerInRange;
    
    private void Start() 
    { 
        if (minigameCanvas != null) minigameCanvas.SetActive(false);
        if (interactionIndicator != null) interactionIndicator.SetActive(false);
        
        if (itemToActivate != null)
            itemToActivate.SetActive(false);
    }
    
    private void Update()
    {
        if (!minigameActive || isCompleted) return;
        
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            currentProgress += clickProgress;
            currentProgress = Mathf.Clamp(currentProgress, 0f, 100f);
            UpdateVisuals();
            
            if (currentProgress >= 100f) CompleteMinigame();
        }
        
        currentProgress -= decayPerSecond * Time.deltaTime;
        currentProgress = Mathf.Clamp(currentProgress, 0f, 100f);
        UpdateVisuals();
    }
    
    private void UpdateVisuals()
    {
        float normalizedProgress = currentProgress / 100f;
        
        if (progressSlider != null)
            progressSlider.value = normalizedProgress;
        
        if (bucketImage != null && bucketStartPoint != null && bucketEndPoint != null)
        {
            float curveValue = bucketCurve.Evaluate(normalizedProgress);
            bucketImage.rectTransform.position = Vector3.Lerp(
                bucketStartPoint.position, 
                bucketEndPoint.position, 
                curveValue
            );
        }
    }
    
    public void StartMinigame()
    {
        if (minigameActive || isCompleted) return;
        if (!playerInRange) return;
        
        minigameActive = true;
        currentProgress = 0f;
        minigameCanvas.SetActive(true);
        progressSlider.value = 0f;
        UpdateVisuals();
        
        if (playerMovement != null) playerMovement.InputBlocked = true;
        if (interactionIndicator != null) interactionIndicator.SetActive(false);
    }
    
    private void CompleteMinigame()
    {
        minigameActive = false;
        isCompleted = true;
        UpdateVisuals();
        minigameCanvas.SetActive(false);
        
        if (itemToActivate != null)
            itemToActivate.SetActive(true);
        
        if (playerMovement != null) playerMovement.InputBlocked = false;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isCompleted) return;
        playerInRange = true;
        
        PlayerCarry playerCarry = other.GetComponent<PlayerCarry>();
        if (playerCarry != null && playerCarry.IsCarryingObject) return;
        if (minigameActive) return;
        
        if (interactionIndicator != null) interactionIndicator.SetActive(true);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        
        if (interactionIndicator != null) interactionIndicator.SetActive(false);
        
        if (minigameActive && !isCompleted)
        {
            minigameActive = false;
            minigameCanvas.SetActive(false);
            if (playerMovement != null) playerMovement.InputBlocked = false;
        }
    }
}