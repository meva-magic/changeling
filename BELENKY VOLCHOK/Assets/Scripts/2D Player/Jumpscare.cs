using UnityEngine;
using System.Collections;

public class FairyJumpscare : MonoBehaviour
{
    [SerializeField] private GameObject fairyVisual;
    [SerializeField] private float appearDuration = 1.5f;
    [SerializeField] private GameObject babyObject;
    [SerializeField] private GameObject changelingPrefab;
    [SerializeField] private GameObject jumpScareUIImage;
    
    public float AppearDuration => appearDuration;
    
    public void TriggerBabySwap() { StartCoroutine(BabySwapSequence()); }
    public void TriggerJumpscare() { StartCoroutine(JumpscareSequence()); }
    
    private IEnumerator JumpscareSequence()
    {
        ZoneCamera zoneCam = ZoneCamera.Instance;
        if (zoneCam != null) yield return zoneCam.FadeToBlack();
        if (fairyVisual != null) fairyVisual.SetActive(true);
        PlayVoiceSound();
        if (zoneCam != null) yield return zoneCam.FadeFromBlack();
        yield return new WaitForSeconds(appearDuration);
        if (zoneCam != null) yield return zoneCam.FadeToBlack();
        if (fairyVisual != null) fairyVisual.SetActive(false);
        if (zoneCam != null) yield return zoneCam.FadeFromBlack();
    }
    
    private IEnumerator BabySwapSequence()
    {
        ZoneCamera zoneCam = ZoneCamera.Instance;
        
        if (zoneCam != null) yield return zoneCam.FadeToBlack();
        if (fairyVisual != null) fairyVisual.SetActive(true);
        PlayVoiceSound();
        yield return new WaitForSeconds(1.5f);
        
        // Save baby position before destroying
        Vector3 babyPosition = babyObject != null ? babyObject.transform.position : Vector3.zero;
        
        // Destroy baby
        if (babyObject != null) Destroy(babyObject);
        
        // Spawn changeling at baby's position
        if (changelingPrefab != null)
            Instantiate(changelingPrefab, babyPosition, Quaternion.identity);
        
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
        
        if (zoneCam != null) yield return zoneCam.FadeToBlack();
        if (jumpScareUIImage != null) jumpScareUIImage.SetActive(true);
        PlayVoiceSound();
        if (zoneCam != null) yield return zoneCam.FadeFromBlack();
        
        yield return new WaitForSeconds(appearDuration);
        
        if (zoneCam != null) yield return zoneCam.FadeToBlack();
        if (jumpScareUIImage != null) jumpScareUIImage.SetActive(false);
        if (zoneCam != null) yield return zoneCam.FadeFromBlack();
    }
    
    private void PlayVoiceSound()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.Play("Voice");
    }
}