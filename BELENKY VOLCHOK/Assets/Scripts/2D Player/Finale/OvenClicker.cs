using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OvenClicker : MonoBehaviour
{
    [Header("Minigame Settings")]
    [SerializeField] private float clickProgress = 2f;
    [SerializeField] private float decayPerSecond = 5f;
    
    [Header("UI")]
    [SerializeField] private GameObject minigameCanvas;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Image ovenImage;
    [SerializeField] private float imageStartY = -200f;
    [SerializeField] private float imageEndY = 200f;
    
    [Header("Completion")]
    [SerializeField] private GameObject jumpscareImage;
    [SerializeField] private float jumpscareDuration = 3f;
    [SerializeField] private string nextSceneName;
    [SerializeField] private string ovenSound = "OvenFire";
    
    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCarry playerCarry;
    
    private float currentProgress;
    private bool minigameActive;
    private bool completed;
    private RectTransform imageRect;
    
    private void Start()
    {
        if (minigameCanvas != null) minigameCanvas.SetActive(false);
        if (jumpscareImage != null) jumpscareImage.SetActive(false);
        if (ovenImage != null) imageRect = ovenImage.GetComponent<RectTransform>();
    }
    
    private void Update()
    {
        if (!minigameActive || completed) return;
        
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            currentProgress += clickProgress;
            currentProgress = Mathf.Clamp(currentProgress, 0f, 100f);
            UpdateUI();
            
            if (currentProgress >= 100f)
                CompleteMinigame();
        }
        
        currentProgress -= decayPerSecond * Time.deltaTime;
        currentProgress = Mathf.Clamp(currentProgress, 0f, 100f);
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        float progress = currentProgress / 100f;
        
        if (progressSlider != null)
            progressSlider.value = progress;
        
        if (imageRect != null)
        {
            float yPos = Mathf.Lerp(imageStartY, imageEndY, progress);
            imageRect.anchoredPosition = new Vector2(imageRect.anchoredPosition.x, yPos);
        }
    }
    
    public void StartMinigame()
    {
        if (minigameActive || completed) return;
        
        minigameActive = true;
        currentProgress = 0f;
        
        if (minigameCanvas != null) minigameCanvas.SetActive(true);
        if (progressSlider != null) progressSlider.value = 0f;
        
        if (imageRect != null)
            imageRect.anchoredPosition = new Vector2(imageRect.anchoredPosition.x, imageStartY);
        
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerCarry != null) playerCarry.enabled = false;
        
        PlaySound(ovenSound);
    }
    
    private void CompleteMinigame()
    {
        completed = true;
        minigameActive = false;
        
        if (minigameCanvas != null) minigameCanvas.SetActive(false);
        
        // Enable player
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerCarry != null) playerCarry.enabled = true;
        
        // Show jumpscare
        StartCoroutine(JumpscareSequence());
    }
    
    private IEnumerator JumpscareSequence()
    {
        if (jumpscareImage != null)
            jumpscareImage.SetActive(true);
        
        PlaySound("Jumpscare");
        
        yield return new WaitForSeconds(jumpscareDuration);
        
        if (!string.IsNullOrEmpty(nextSceneName))
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }
    
    private void PlaySound(string soundName)
    {
        if (string.IsNullOrEmpty(soundName)) return;
        if (AudioManager.instance != null)
            AudioManager.instance.Play(soundName);
    }
}