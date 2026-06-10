using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OvenFinale : MonoBehaviour
{
    [SerializeField] private OvenMinigame ovenMinigame;
    [SerializeField] private PlayerController playerController;

    [Header("Audio")]
    [SerializeField] private string finaleSound = "";
    [SerializeField] private string newspaperAmbience = "";
    [SerializeField] private string creditsAmbience = "";

    [Header("Newspaper")]
    [SerializeField] private GameObject newspaperPanel;
    [SerializeField] private float newspaperMinDuration = 3f;
    [SerializeField] private float newspaperFadeInTime = 1.5f;
    [SerializeField] private float newspaperFadeOutTime = 1.5f;
    [SerializeField] private GameObject newspaperContinueHint;

    [Header("Credits")]
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private ScrollRect creditsScrollRect;
    [SerializeField] private float creditsScrollSpeed = 30f;
    [SerializeField] private float creditsFadeInTime = 1.5f;
    [SerializeField] private GameObject mainMenuButton;

    private bool newspaperClosed;
    private float newspaperShowTime;

    private void Start()
    {
        if (newspaperPanel != null) newspaperPanel.SetActive(false);
        if (newspaperContinueHint != null) newspaperContinueHint.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.SetActive(false);

        if (ovenMinigame != null)
            ovenMinigame.OnMinigameCompleted += StartFinale;
    }

    private void Update()
    {
        if (newspaperPanel != null && newspaperPanel.activeSelf && !newspaperClosed)
        {
            if (newspaperContinueHint != null && !newspaperContinueHint.activeSelf)
            {
                if (Time.time - newspaperShowTime >= newspaperMinDuration)
                    newspaperContinueHint.SetActive(true);
            }

            if (Time.time - newspaperShowTime >= newspaperMinDuration &&
                (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
                StartCoroutine(CloseNewspaper());
            return;
        }

        if (creditsPanel != null && creditsPanel.activeSelf && creditsScrollRect != null)
            creditsScrollRect.verticalNormalizedPosition -= creditsScrollSpeed * Time.deltaTime / 1000f;
    }

    private void StartFinale()
    {
        if (!string.IsNullOrEmpty(finaleSound) && AudioManager.instance != null)
            AudioManager.instance.Play(finaleSound);

        StartCoroutine(ShowNewspaper());
    }

    private IEnumerator ShowNewspaper()
    {
        ScreenFader fader = ScreenFader.Instance;
        if (fader != null)
        {
            fader.SetAlpha(1f);
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (newspaperPanel != null)
            newspaperPanel.SetActive(true);

        if (fader != null)
        {
            float elapsed = 0;
            while (elapsed < newspaperFadeInTime)
            {
                elapsed += Time.deltaTime;
                fader.SetAlpha(1f - (elapsed / newspaperFadeInTime));
                yield return null;
            }
            fader.SetAlpha(0f);
        }

        if (!string.IsNullOrEmpty(newspaperAmbience) && AudioManager.instance != null)
            AudioManager.instance.Play(newspaperAmbience);

        newspaperShowTime = Time.time;
        newspaperClosed = false;
    }

    private IEnumerator CloseNewspaper()
    {
        newspaperClosed = true;

        if (newspaperContinueHint != null)
            newspaperContinueHint.SetActive(false);

        ScreenFader fader = ScreenFader.Instance;
        if (fader != null)
        {
            float elapsed = 0;
            while (elapsed < newspaperFadeOutTime)
            {
                elapsed += Time.deltaTime;
                fader.SetAlpha(elapsed / newspaperFadeOutTime);
                yield return null;
            }
            fader.SetAlpha(1f);
        }

        if (newspaperPanel != null)
            newspaperPanel.SetActive(false);

        if (!string.IsNullOrEmpty(newspaperAmbience) && AudioManager.instance != null)
            AudioManager.instance.Stop(newspaperAmbience);

        if (creditsPanel != null)
            creditsPanel.SetActive(true);

        if (fader != null)
        {
            float elapsed = 0;
            while (elapsed < creditsFadeInTime)
            {
                elapsed += Time.deltaTime;
                fader.SetAlpha(1f - (elapsed / creditsFadeInTime));
                yield return null;
            }
            fader.SetAlpha(0f);
        }

        if (mainMenuButton != null) mainMenuButton.SetActive(true);
        if (creditsScrollRect != null) creditsScrollRect.verticalNormalizedPosition = 1f;

        if (!string.IsNullOrEmpty(creditsAmbience) && AudioManager.instance != null)
            AudioManager.instance.Play(creditsAmbience);

        if (playerController != null) playerController.InputBlocked = false;
    }

    public void LoadMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}