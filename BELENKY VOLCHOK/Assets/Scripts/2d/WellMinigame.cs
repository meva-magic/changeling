using UnityEngine;
using UnityEngine.UI;

public class WellMinigame : MonoBehaviour
{
    [SerializeField] private GameObject itemToActivate;
    [SerializeField] private float clickProgress = 2f;
    [SerializeField] private float decayPerSecond = 5f;
    [SerializeField] private GameObject minigameCanvas;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private PlayerController playerController;

    [Header("Animation")]
    [SerializeField] private Image bucketImage;
    [SerializeField] private RectTransform bucketStartPoint;
    [SerializeField] private RectTransform bucketEndPoint;
    [SerializeField] private AnimationCurve bucketCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Indicator")]
    [SerializeField] private GameObject interactionIndicator;

    [Header("Audio")]
    [SerializeField] private string startSound = "";
    [SerializeField] private string completeSound = "";
    [SerializeField] private string clickSound = "";

    private float currentProgress;
    private bool minigameActive;
    private bool isCompleted;
    private bool playerInRange;

    public bool IsCompleted => isCompleted;

    private void Start()
    {
        if (minigameCanvas != null) minigameCanvas.SetActive(false);
        if (interactionIndicator != null) interactionIndicator.SetActive(false);
        if (itemToActivate != null) itemToActivate.SetActive(false);
    }

    private void Update()
    {
        if (!minigameActive || isCompleted) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelMinigame();
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            currentProgress += clickProgress;
            currentProgress = Mathf.Clamp(currentProgress, 0f, 100f);
            UpdateVisuals();

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
        if (progressSlider != null) progressSlider.value = progress;

        if (bucketImage != null && bucketStartPoint != null && bucketEndPoint != null)
        {
            float curveValue = bucketCurve.Evaluate(progress);
            bucketImage.rectTransform.position = Vector3.Lerp(
                bucketStartPoint.position, bucketEndPoint.position, curveValue);
        }
    }

    public void StartMinigame()
    {
        if (minigameActive || isCompleted || !playerInRange) return;

        minigameActive = true;
        currentProgress = 0f;
        minigameCanvas.SetActive(true);
        progressSlider.value = 0f;
        UpdateVisuals();

        if (playerController != null) playerController.InputBlocked = true;
        if (interactionIndicator != null) interactionIndicator.SetActive(false);

        CameraShake shake = Camera.main != null ? Camera.main.GetComponent<CameraShake>() : null;
        if (shake != null) shake.ShakeOnce();

        if (!string.IsNullOrEmpty(startSound) && AudioManager.instance != null)
            AudioManager.instance.Play(startSound);
    }

    private void CancelMinigame()
    {
        minigameActive = false;
        currentProgress = 0f;
        minigameCanvas.SetActive(false);
        progressSlider.value = 0f;
        UpdateVisuals();

        if (playerController != null) playerController.InputBlocked = false;
    }

    private void Complete()
    {
        minigameActive = false;
        isCompleted = true;
        minigameCanvas.SetActive(false);

        if (itemToActivate != null) itemToActivate.SetActive(true);
        if (playerController != null) playerController.InputBlocked = false;

        CameraShake shake = Camera.main != null ? Camera.main.GetComponent<CameraShake>() : null;
        if (shake != null) shake.ShakeOnce();

        if (!string.IsNullOrEmpty(completeSound) && AudioManager.instance != null)
            AudioManager.instance.Play(completeSound);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isCompleted) return;
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
            currentProgress = 0f;
            minigameCanvas.SetActive(false);
            progressSlider.value = 0f;
            if (playerController != null) playerController.InputBlocked = false;
        }
    }
}