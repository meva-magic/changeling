using UnityEngine;
using System.Collections;

public class FairyJumpscare : MonoBehaviour
{
    [SerializeField] private GameObject fairyVisual;
    [SerializeField] private float appearDuration = 1.5f;
    [SerializeField] private AudioClip jumpscareSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject babyObject;
    [SerializeField] private GameObject changelingPrefab;
    [SerializeField] private Transform changelingSpawnPoint;
    [SerializeField] private GameObject jumpScareUIImage;
    
    public float AppearDuration => appearDuration;
    
    public void TriggerBabySwap()
    {
        StartCoroutine(BabySwapSequence());
    }
    
    public void TriggerJumpscare()
    {
        StartCoroutine(JumpscareSequence());
    }
    
    private IEnumerator JumpscareSequence()
    {
        ZoneCamera zoneCam = ZoneCamera.Instance;
        
        if (zoneCam != null) yield return zoneCam.FadeToBlack();
        if (fairyVisual != null) fairyVisual.SetActive(true);
        PlaySound();
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
        PlaySound();
        yield return new WaitForSeconds(1.5f);
        
        if (babyObject != null) Destroy(babyObject);
        if (changelingPrefab != null && changelingSpawnPoint != null)
            Instantiate(changelingPrefab, changelingSpawnPoint.position, Quaternion.identity);
        
        if (fairyVisual != null) fairyVisual.SetActive(false);
        if (zoneCam != null) yield return zoneCam.FadeFromBlack();
    }
    
    private void PlaySound()
    {
        if (audioSource != null && jumpscareSound != null)
            audioSource.PlayOneShot(jumpscareSound);
    }
}