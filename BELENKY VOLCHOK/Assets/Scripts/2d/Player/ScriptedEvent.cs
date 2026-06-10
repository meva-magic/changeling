using UnityEngine;
using System.Collections;

public class ScriptedEvent : MonoBehaviour
{
    [SerializeField] private GameObject fairyVisual;
    [SerializeField] private float appearDuration = 1.5f;
    [SerializeField] private GameObject babyObject;
    [SerializeField] private GameObject changelingObject;
    [SerializeField] private GameObject screamerImage;
    [SerializeField] private string fairySound = "";

    [Header("Oven")]
    [SerializeField] private GameObject ovenIndicator;
    [SerializeField] private bool ovenEnabled = true;

    private bool playerInRange;

    private void Start()
    {
        if (ovenIndicator != null) ovenIndicator.SetActive(false);
        if (changelingObject != null) changelingObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;

        if (ovenEnabled && ovenIndicator != null)
        {
            PlayerCarry playerCarry = other.GetComponent<PlayerCarry>();
            if (playerCarry != null && playerCarry.IsCarryingObject) return;
            ovenIndicator.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (ovenIndicator != null) ovenIndicator.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange || !ovenEnabled) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsShowing) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            PlayerCarry playerCarry = FindObjectOfType<PlayerCarry>();
            if (playerCarry != null && playerCarry.IsCarryingObject) return;
            OvenJumpscare();
            ovenEnabled = false;
            if (ovenIndicator != null) ovenIndicator.SetActive(false);
        }
    }

    public void SwapBabyForChangeling()
    {
        StartCoroutine(SwapSequence());
    }

    private IEnumerator SwapSequence()
    {
        PlayFairySound();
        yield return new WaitForSeconds(0.5f);

        if (babyObject != null && changelingObject != null)
        {
            changelingObject.transform.position = babyObject.transform.position;
            babyObject.SetActive(false);
            changelingObject.SetActive(true);
        }
    }

    public void OvenJumpscare()
    {
        StartCoroutine(OvenSequence());
    }

    private IEnumerator OvenSequence()
    {
        ScreenFader fader = ScreenFader.Instance;
        if (fader == null) fader = FindObjectOfType<ScreenFader>();

        if (fader != null) yield return fader.FadeToBlack();
        if (screamerImage != null) screamerImage.SetActive(true);
        PlayFairySound();
        if (fader != null) yield return fader.FadeFromBlack();
        yield return new WaitForSeconds(appearDuration);
        if (fader != null) yield return fader.FadeToBlack();
        if (screamerImage != null) screamerImage.SetActive(false);
        if (fader != null) yield return fader.FadeFromBlack();
    }

    private void PlayFairySound()
    {
        if (!string.IsNullOrEmpty(fairySound) && AudioManager.instance != null)
            AudioManager.instance.Play(fairySound);
    }
}