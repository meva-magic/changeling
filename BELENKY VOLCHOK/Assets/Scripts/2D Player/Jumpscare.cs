using UnityEngine;
using System.Collections;

public class FairyJumpscare : MonoBehaviour
{
    [SerializeField] private GameObject fairyVisual;
    [SerializeField] private float appearDuration = 1.5f;
    [SerializeField] private GameObject babyObject;
    [SerializeField] private GameObject changelingObject;
    [SerializeField] private GameObject jumpScareUIImage;
    [SerializeField] private string fairySoundName = "FairyVoice";
    
    [Header("Oven Interaction")]
    [SerializeField] private GameObject ovenInteractionIndicator;
    [SerializeField] private bool isOvenInteractable = true;
    
    private bool playerInRange;
    
    public float AppearDuration => appearDuration;
    
    private void Start()
    {
        if (ovenInteractionIndicator != null)
            ovenInteractionIndicator.SetActive(false);
        
        if (changelingObject != null)
            changelingObject.SetActive(false);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        playerInRange = true;
        
        if (isOvenInteractable && ovenInteractionIndicator != null)
        {
            PlayerCarry playerCarry = other.GetComponent<PlayerCarry>();
            if (playerCarry != null && playerCarry.IsCarryingObject)
                return;
            
            ovenInteractionIndicator.SetActive(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        
        if (ovenInteractionIndicator != null)
            ovenInteractionIndicator.SetActive(false);
    }
    
    private void Update()
    {
        if (!playerInRange) return;
        if (!isOvenInteractable) return;
        if (SimpleDialogueManager.Instance != null && SimpleDialogueManager.Instance.IsShowing) return;
        
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            PlayerCarry playerCarry = FindObjectOfType<PlayerCarry>();
            if (playerCarry != null && playerCarry.IsCarryingObject) return;
            
            TriggerOvenJumpscare();
            isOvenInteractable = false;
            
            if (ovenInteractionIndicator != null)
                ovenInteractionIndicator.SetActive(false);
        }
    }
    
    public void SwapBabyOnly()
    {
        StartCoroutine(BabySwapSequence());
    }
    
    private IEnumerator BabySwapSequence()
    {
        ZoneCamera zoneCam = ZoneCamera.Instance;
        if (zoneCam == null) zoneCam = FindObjectOfType<ZoneCamera>();
        
        if (zoneCam != null) yield return zoneCam.FadeToBlack();
        
        PlaySound();
        yield return new WaitForSeconds(1f);
        
        if (babyObject != null && changelingObject != null)
        {
            changelingObject.transform.position = babyObject.transform.position;
            babyObject.SetActive(false);
            changelingObject.SetActive(true);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        if (zoneCam != null) yield return zoneCam.FadeFromBlack();
    }
    
    public void TriggerJumpscare()
    {
        StartCoroutine(JumpscareSequence());
    }
    
    private IEnumerator JumpscareSequence()
    {
        ZoneCamera zoneCam = ZoneCamera.Instance;
        if (zoneCam == null) zoneCam = FindObjectOfType<ZoneCamera>();
        
        if (zoneCam != null) yield return zoneCam.FadeToBlack();
        if (fairyVisual != null) fairyVisual.SetActive(true);
        PlaySound();
        if (zoneCam != null) yield return zoneCam.FadeFromBlack();
        yield return new WaitForSeconds(appearDuration);
        if (zoneCam != null) yield return zoneCam.FadeToBlack();
        if (fairyVisual != null) fairyVisual.SetActive(false);
        if (zoneCam != null) yield return zoneCam.FadeFromBlack();
    }
    
    public void TriggerOvenJumpscare()
    {
        StartCoroutine(OvenJumpscareSequence());
    }
    
    private IEnumerator OvenJumpscareSequence()
    {
        ZoneCamera zoneCam = ZoneCamera.Instance;
        if (zoneCam == null) zoneCam = FindObjectOfType<ZoneCamera>();
        
        if (zoneCam != null) yield return zoneCam.FadeToBlack();
        if (jumpScareUIImage != null) jumpScareUIImage.SetActive(true);
        PlaySound();
        if (zoneCam != null) yield return zoneCam.FadeFromBlack();
        yield return new WaitForSeconds(appearDuration);
        if (zoneCam != null) yield return zoneCam.FadeToBlack();
        if (jumpScareUIImage != null) jumpScareUIImage.SetActive(false);
        if (zoneCam != null) yield return zoneCam.FadeFromBlack();
    }
    
    private void PlaySound()
    {
        if (AudioManager.instance != null && !string.IsNullOrEmpty(fairySoundName))
            AudioManager.instance.Play(fairySoundName);
    }
}