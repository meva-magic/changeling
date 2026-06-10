using UnityEngine;
using UnityEngine.UI;

public class OvenMinigame : MonoBehaviour
{
    [SerializeField] private float clickProgress = 2f;
    [SerializeField] private float decayPerSecond = 5f;
    [SerializeField] private GameObject minigameCanvas;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private OvenAmbience ovenAmbience;
    [SerializeField] private CameraShake cameraShake;

    [Header("Changeling Animation")]
    [SerializeField] private RectTransform changelingObject;
    [SerializeField] private RectTransform changelingStartPoint;
    [SerializeField] private RectTransform changelingEndPoint;

    [Header("Red Overlay")]
    [SerializeField] private Image redOverlayImage;
    [SerializeField] private float maxOverlayAlpha = 0.6f;

    [Header("Audio")]
    [SerializeField] private string startSound = "";
    [SerializeField] private string clickSound = "";
    [SerializeField] private string completeSound = "";

    [Header("Indicator")]
    [SerializeField] private GameObject interactionIndicator;

    private float currentProgress;
    private bool minigameActive;
    private bool isCompleted;
    private bool playerInRange;

    public bool IsCompleted => isCompleted;

    public event System.Action OnMinigameCompleted;

    private void Start()
    {
        if (minigameCanvas != null) minigameCanvas.SetActive(false);
        if (interactionIndicator != null) interactionIndicator.SetActive(false);

        if (redOverlayImage != null)
        {
            Color c = redOverlayImage.color;
            c.a = 0;
            redOverlayImage.color = c;
        }

        if (cameraShake == null)
            cameraShake = Camera.main != null ? Camera.main.GetComponent<CameraShake>() : null;
    }

    private void Update()
    {
        if (!minigameActive || isCompleted) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            currentProgress += clickProgress;
            currentProgress = Mathf.Clamp(currentProgress, 0f, 100f);
            UpdateVisuals();

            if (cameraShake != null)
                cameraShake.ShakeOnce();

            if (!string.IsNullOrEmpty(clickSound) && AudioManager.instance != null)
                AudioManager.instance.Play(clickSound);

            if (currentProgress >= 100f) Complete();
        }

        currentProgress -= decayPerSecond * Time.deltaTime;
        currentProgress = Mathf.Clamp(currentProgress, 0f, 100f);
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        float progress = currentProgress / 100f;

        if (progressSlider != null)
            progressSlider.value = progress;

        if (changelingObject != null && changelingStartPoint != null && changelingEndPoint != null)
        {
            changelingObject.position = Vector3.Lerp(
                changelingStartPoint.position,
                changelingEndPoint.position,
                progress);
        }

        if (redOverlayImage != null)
        {
            Color c = redOverlayImage.color;
            c.a = Mathf.Lerp(0, maxOverlayAlpha, progress);
            redOverlayImage.color = c;
        }
    }

    public void StartMinigame()
    {
        if (minigameActive || isCompleted || !playerInRange) return;

        if (cameraShake == null)
            cameraShake = Camera.main != null ? Camera.main.GetComponent<CameraShake>() : null;

        minigameActive = true;
        currentProgress = 0f;
        minigameCanvas.SetActive(true);
        progressSlider.value = 0f;
        UpdateVisuals();

        if (playerController != null) playerController.InputBlocked = true;
        if (interactionIndicator != null) interactionIndicator.SetActive(false);
        if (ovenAmbience != null) ovenAmbience.ActivateOvenAmbience();

        if (cameraShake != null)
            cameraShake.ShakeOnce();

        if (!string.IsNullOrEmpty(startSound) && AudioManager.instance != null)
            AudioManager.instance.Play(startSound);
    }

    private void Complete()
    {
        minigameActive = false;
        isCompleted = true;
        minigameCanvas.SetActive(false);

        if (ovenAmbience != null) ovenAmbience.StopAll();

        if (!string.IsNullOrEmpty(completeSound) && AudioManager.instance != null)
            AudioManager.instance.Play(completeSound);

        if (playerController != null) playerController.InputBlocked = true;

        OnMinigameCompleted?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isCompleted) return;
        playerInRange = true;
        if (minigameActive) return;
        if (interactionIndicator != null) interactionIndicator.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (interactionIndicator != null) interactionIndicator.SetActive(false);
    }
}