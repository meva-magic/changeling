using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ClickerMinigameSystem : MonoBehaviour
{
    public static ClickerMinigameSystem Instance { get; private set; }
    
    [Header("UI References")]
    public GameObject minigamePanel;
    public Slider fillSlider;
    public Image fillImage;
    public Sprite fillTopSprite;
    public Image matchImage;
    public RectTransform matchStartPoint;
    public RectTransform matchEndPoint;
    
    [Header("Animation Settings")]
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Visual Effects (Optional)")]
    public ParticleSystem fireParticleEffect;
    public Transform effectSpawnPoint;
    
    [Header("Audio Settings (Optional - leave empty to skip)")]
    public string clickSoundName = "";
    public string completeSoundName = "";
    public string cancelSoundName = "";
    public string fireSoundName = "";
    
    private MinigameData currentMinigame;
    private float currentProgress;
    private bool isMinigameActive;
    private bool waitingForFireEffect;
    private Vector3 savedPlayerPosition;
    private Quaternion savedPlayerRotation;
    private bool playerPositionSaved;
    
    [System.Serializable]
    public class MinigameData
    {
        public string minigameId;
        public float weightPerClick = 10f;
        public float decayRate = 0f;
        public System.Action onComplete;
        public System.Action onCancel;
        public GameObject targetObject;
        public bool showMatchAnimation = true;
        public bool showFireEffect = true;
    }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (minigamePanel != null)
            minigamePanel.SetActive(false);
    }
    
    private void Update()
    {
        if (!isMinigameActive || waitingForFireEffect) return;
        
        // Handle clicks during minigame
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            OnClick();
        }
        
        // Handle decay
        if (currentMinigame != null && currentMinigame.decayRate > 0 && currentProgress > 0 && !waitingForFireEffect)
        {
            currentProgress -= currentMinigame.decayRate * Time.deltaTime;
            if (currentProgress < 0) currentProgress = 0;
            
            float normalizedProgress = currentProgress / 100f;
            UpdateSliderAndMatch(normalizedProgress);
            
            if (currentProgress <= 0)
            {
                CancelMinigame();
            }
        }
        
        // Cancel with escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelMinigame();
        }
    }
    
    public void StartMinigame(MinigameData data)
    {
        Debug.Log("StartMinigame called");
        
        if (isMinigameActive)
        {
            Debug.Log("Minigame already active");
            return;
        }
        
        if (data == null)
        {
            Debug.LogError("MinigameData is null!");
            return;
        }
        
        currentMinigame = data;
        currentProgress = 0;
        isMinigameActive = true;
        waitingForFireEffect = false;
        
        // Save and freeze player completely
        FreezePlayer();
        
        if (minigamePanel != null)
        {
            minigamePanel.SetActive(true);
            Debug.Log("Minigame panel activated");
        }
        else
        {
            Debug.LogError("minigamePanel is null!");
            return;
        }
        
        UpdateSliderAndMatch(0);
        
        if (UIMessageManager.Instance != null)
            UIMessageManager.Instance.HideMessage();
    }
    
    private void OnClick()
    {
        if (!isMinigameActive || waitingForFireEffect) return;
        
        currentProgress = Mathf.Min(currentProgress + currentMinigame.weightPerClick, 100f);
        float normalizedProgress = currentProgress / 100f;
        UpdateSliderAndMatch(normalizedProgress);
        
        if (!string.IsNullOrEmpty(clickSoundName))
            PlaySound(clickSoundName);
        
        if (currentProgress >= 100f)
        {
            CompleteMinigame();
        }
    }
    
    private void UpdateSliderAndMatch(float normalizedProgress)
    {
        if (fillSlider != null)
            fillSlider.value = normalizedProgress;
        
        if (fillImage != null && fillTopSprite != null)
        {
            fillImage.sprite = fillTopSprite;
            fillImage.fillAmount = normalizedProgress;
        }
        
        if (matchImage != null && matchStartPoint != null && matchEndPoint != null)
        {
            Vector3 startPos = matchStartPoint.position;
            Vector3 endPos = matchEndPoint.position;
            matchImage.rectTransform.position = Vector3.Lerp(startPos, endPos, normalizedProgress);
        }
    }
    
    private void CompleteMinigame()
    {
        Debug.Log("CompleteMinigame called");
        
        if (currentMinigame == null)
        {
            Debug.LogError("currentMinigame is null in CompleteMinigame!");
            CloseMinigamePanel();
            return;
        }
        
        isMinigameActive = false;
        
        if (!string.IsNullOrEmpty(completeSoundName))
            PlaySound(completeSoundName);
        
        var onCompleteCallback = currentMinigame.onComplete;
        
        if (currentMinigame.showFireEffect && fireParticleEffect != null)
        {
            Vector3 spawnPos = effectSpawnPoint != null ? effectSpawnPoint.position : 
                              (currentMinigame.targetObject != null ? currentMinigame.targetObject.transform.position : Vector3.zero);
            
            ParticleSystem fire = Instantiate(fireParticleEffect, spawnPos, Quaternion.identity);
            if (fire != null)
            {
                fire.Play();
                Destroy(fire.gameObject, 3f);
            }
            
            if (!string.IsNullOrEmpty(fireSoundName))
                PlaySound(fireSoundName);
            
            waitingForFireEffect = true;
            StartCoroutine(DelayedClose(0.5f, onCompleteCallback));
        }
        else
        {
            CloseMinigamePanel();
            onCompleteCallback?.Invoke();
            UnfreezePlayer();
        }
    }
    
    private IEnumerator DelayedClose(float delay, System.Action onCompleteCallback)
    {
        yield return new WaitForSeconds(delay);
        CloseMinigamePanel();
        onCompleteCallback?.Invoke();
        UnfreezePlayer();
        waitingForFireEffect = false;
    }
    
    private void CloseMinigamePanel()
    {
        Debug.Log("CloseMinigamePanel called");
        
        if (minigamePanel != null)
            minigamePanel.SetActive(false);
        
        currentMinigame = null;
        currentProgress = 0;
        isMinigameActive = false;
    }
    
    public void CancelMinigame()
    {
        if (!isMinigameActive || waitingForFireEffect) return;
        
        Debug.Log("CancelMinigame called");
        isMinigameActive = false;
        
        if (minigamePanel != null)
            minigamePanel.SetActive(false);
        
        if (!string.IsNullOrEmpty(cancelSoundName))
            PlaySound(cancelSoundName);
        
        var onCancelCallback = currentMinigame?.onCancel;
        currentMinigame = null;
        currentProgress = 0;
        
        onCancelCallback?.Invoke();
        UnfreezePlayer();
    }
    
    private void FreezePlayer()
    {
        Debug.Log("Freezing player");
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = FindObjectOfType<MouseLook>()?.gameObject;
        }
        
        if (player != null)
        {
            savedPlayerPosition = player.transform.position;
            savedPlayerRotation = player.transform.rotation;
            playerPositionSaved = true;
            
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }
            
            MouseLook mouseLook = player.GetComponent<MouseLook>();
            if (mouseLook != null) mouseLook.enabled = false;
            
            PlayerMove playerMove = player.GetComponent<PlayerMove>();
            if (playerMove != null) playerMove.enabled = false;
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        PlayerStateManager stateManager = FindObjectOfType<PlayerStateManager>();
        if (stateManager != null) stateManager.SetUIState();
    }
    
    private void UnfreezePlayer()
    {
        Debug.Log("Unfreezing player");
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = FindObjectOfType<MouseLook>()?.gameObject;
        }
        
        if (player != null)
        {
            if (playerPositionSaved)
            {
                player.transform.position = savedPlayerPosition;
                player.transform.rotation = savedPlayerRotation;
                playerPositionSaved = false;
            }
            
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;
            
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = true;
            
            MouseLook mouseLook = player.GetComponent<MouseLook>();
            if (mouseLook != null) mouseLook.enabled = true;
            
            PlayerMove playerMove = player.GetComponent<PlayerMove>();
            if (playerMove != null) playerMove.enabled = true;
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        PlayerStateManager stateManager = FindObjectOfType<PlayerStateManager>();
        if (stateManager != null) stateManager.SetGameplayState();
    }
    
    private void PlaySound(string soundName)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.Play(soundName);
    }
    
    public bool IsMinigameActive => isMinigameActive;
}