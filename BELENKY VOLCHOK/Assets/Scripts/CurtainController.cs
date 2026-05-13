using UnityEngine;
using UnityEngine.UI;

public class CurtainController : MonoBehaviour, IInteractable
{
    [Header("Models")]
    public GameObject openModel;
    public GameObject closedModel;
    
    [Header("Hold Settings")]
    public float holdTimeRequired = 3f;
    public float decayRate = 1f;
    public float fillRate = 1f;
    
    [Header("UI")]
    public GameObject progressPanel;
    public Slider progressSlider;
    public Image fillImage;
    
    [Header("Target")]
    public WindowPoint targetWindow;
    
    [Header("Audio")]
    public string holdStartSoundName = "CurtainHoldStart";
    public string closeCompleteSoundName = "CurtainClose";
    public string cancelSoundName = "CurtainCancel";
    
    private float currentProgress;
    private bool isHolding;
    private bool isClosed;
    private Camera mainCamera;
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        if (openModel != null) openModel.SetActive(true);
        if (closedModel != null) closedModel.SetActive(false);
        if (progressPanel != null) progressPanel.SetActive(false);
        
        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = holdTimeRequired;
        }
    }
    
    private void Update()
    {
        if (isClosed) return;
        
        bool looking = IsLookingAtCurtain();
        bool hasMonster = targetWindow != null && targetWindow.HasActiveMonster;
        
        if (looking && hasMonster && Input.GetMouseButton(0))
        {
            if (!isHolding)
            {
                isHolding = true;
                if (progressPanel != null) progressPanel.SetActive(true);
                PlaySound(holdStartSoundName);
            }
            
            currentProgress += fillRate * Time.deltaTime;
            currentProgress = Mathf.Min(currentProgress, holdTimeRequired);
            UpdateUI();
            
            if (currentProgress >= holdTimeRequired)
            {
                CloseCurtain();
            }
        }
        else
        {
            if (isHolding)
            {
                isHolding = false;
                if (currentProgress < holdTimeRequired && currentProgress > 0)
                    PlaySound(cancelSoundName);
            }
            
            if (currentProgress > 0)
            {
                currentProgress -= decayRate * Time.deltaTime;
                currentProgress = Mathf.Max(currentProgress, 0);
                UpdateUI();
                
                if (currentProgress <= 0 && progressPanel != null)
                    progressPanel.SetActive(false);
            }
        }
    }
    
    private bool IsLookingAtCurtain()
    {
        if (mainCamera == null) return false;
        
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 5f))
        {
            return hit.collider.gameObject == gameObject;
        }
        return false;
    }
    
    private void UpdateUI()
    {
        if (progressSlider != null)
        {
            progressSlider.value = currentProgress;
            if (fillImage != null)
            {
                float t = currentProgress / holdTimeRequired;
                fillImage.color = Color.Lerp(Color.red, Color.green, t);
            }
        }
    }
    
    private void CloseCurtain()
    {
        isClosed = true;
        isHolding = false;
        
        if (openModel != null) openModel.SetActive(false);
        if (closedModel != null) closedModel.SetActive(true);
        if (progressPanel != null) progressPanel.SetActive(false);
        
        PlaySound(closeCompleteSoundName);
        
        if (targetWindow != null && targetWindow.HasActiveMonster)
            targetWindow.DespawnMonster();
    }
    
    private void PlaySound(string soundName)
    {
        if (!string.IsNullOrEmpty(soundName) && AudioManager.instance != null)
            AudioManager.instance.Play(soundName);
    }
    
    public void Interact()
    {
        // Optional: Add interaction hint
    }
    
    public string GetInteractionName()
    {
        return "Curtain (Hold to Close)";
    }
    
    public void ResetCurtain()
    {
        isClosed = false;
        currentProgress = 0;
        
        if (openModel != null) openModel.SetActive(true);
        if (closedModel != null) closedModel.SetActive(false);
        if (progressPanel != null) progressPanel.SetActive(false);
        if (progressSlider != null) progressSlider.value = 0;
    }
}