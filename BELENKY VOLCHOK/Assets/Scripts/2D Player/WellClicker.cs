using UnityEngine;
using UnityEngine.UI;

public class ClickerMinigame : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float clickProgress = 2f;
    [SerializeField] private float decayPerSecond = 5f;
    [SerializeField] private GameObject minigameCanvas;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private PlayerMovement playerMovement;
    
    private float currentProgress;
    private bool minigameActive;
    private bool isCompleted;
    
    private void Start() 
    { 
        if (minigameCanvas != null) minigameCanvas.SetActive(false); 
    }
    
    private void Update()
    {
        if (!minigameActive || isCompleted) return;
        
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            currentProgress += clickProgress;
            currentProgress = Mathf.Clamp(currentProgress, 0f, 100f);
            if (progressSlider != null) progressSlider.value = currentProgress / 100f;
            
            if (currentProgress >= 100f) CompleteMinigame();
        }
        
        currentProgress -= decayPerSecond * Time.deltaTime;
        currentProgress = Mathf.Clamp(currentProgress, 0f, 100f);
        if (progressSlider != null) progressSlider.value = currentProgress / 100f;
    }
    
    public void StartMinigame()
    {
        if (minigameActive || isCompleted) return;
        
        minigameActive = true;
        currentProgress = 0f;
        minigameCanvas.SetActive(true);
        progressSlider.value = 0f;
        if (playerMovement != null) playerMovement.InputBlocked = true;
    }
    
    private void CompleteMinigame()
    {
        minigameActive = false;
        isCompleted = true;
        minigameCanvas.SetActive(false);
        
        if (itemPrefab != null && spawnPoint != null)
            Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
        
        if (playerMovement != null) playerMovement.InputBlocked = false;
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        if (minigameActive && !isCompleted)
        {
            minigameActive = false;
            minigameCanvas.SetActive(false);
            if (playerMovement != null) playerMovement.InputBlocked = false;
        }
    }
}