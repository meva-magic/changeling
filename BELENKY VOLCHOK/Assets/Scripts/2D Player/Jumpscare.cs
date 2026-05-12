using UnityEngine;
using System.Collections;

public class FairyJumpscare : MonoBehaviour
{
    [Header("Fairy Visual")]
    [SerializeField] private GameObject fairyVisual;
    [SerializeField] private float appearDuration = 1.5f;
    
    [Header("Baby Swap")]
    [SerializeField] private GameObject babyObject;
    [SerializeField] private GameObject changelingPrefab;
    
    [Header("Jumpscare Image")]
    [SerializeField] private GameObject jumpscareImage;
    [SerializeField] private float jumpscareDuration = 2f;
    
    [Header("Next Scene")]
    [SerializeField] private string nextSceneName;
    
    [Header("Audio")]
    [SerializeField] private string fairyVoiceSound = "FairyVoice";
    [SerializeField] private string jumpscareSound = "Jumpscare";
    
    private bool babySwapped;
    private GameObject spawnedChangeling;
    private bool sceneLoading;
    
    private void Start()
    {
        if (fairyVisual != null) fairyVisual.SetActive(false);
        if (jumpscareImage != null) jumpscareImage.SetActive(false);
    }
    
    private void Update()
    {
        if (!babySwapped || sceneLoading) return;
        
        if (spawnedChangeling != null)
        {
            PickupableItem item = spawnedChangeling.GetComponent<PickupableItem>();
            if (item != null && item.IsBeingCarried)
            {
                sceneLoading = true;
                Debug.Log("[Fairy] Changeling picked up! Show jumpscare!");
                StartCoroutine(JumpscareSequence());
            }
        }
    }
    
    public void OnLeaveZone2()
    {
        StartCoroutine(BabySwapSequence());
    }
    
    public void OnReturnToZone2()
    {
        StartCoroutine(FairyAppearsSequence());
    }
    
    private IEnumerator BabySwapSequence()
    {
        if (babyObject == null || changelingPrefab == null) yield break;
        
        Vector3 babyPos = babyObject.transform.position;
        Destroy(babyObject);
        spawnedChangeling = Instantiate(changelingPrefab, babyPos, Quaternion.identity);
        babySwapped = true;
    }
    
    private IEnumerator FairyAppearsSequence()
    {
        if (!babySwapped || fairyVisual == null) yield break;
        
        ZoneCamera zoneCam = FindZoneCamera();
        
        if (zoneCam != null) yield return zoneCam.FadeToBlack();
        fairyVisual.SetActive(true);
        PlaySound(fairyVoiceSound);
        if (zoneCam != null) yield return zoneCam.FadeFromBlack();
        
        yield return new WaitForSeconds(appearDuration);
        
        if (zoneCam != null) yield return zoneCam.FadeToBlack();
        fairyVisual.SetActive(false);
        if (zoneCam != null) yield return zoneCam.FadeFromBlack();
    }
    
    // INSTANT jumpscare - NO fade, just pop up
    private IEnumerator JumpscareSequence()
    {
        // Show jumpscare instantly
        if (jumpscareImage != null)
        {
            jumpscareImage.SetActive(true);
            Debug.Log("[Fairy] Jumpscare image SHOWN!");
        }
        
        PlaySound(jumpscareSound);
        
        // Wait for duration
        yield return new WaitForSeconds(jumpscareDuration);
        
        // Load next scene
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"[Fairy] Loading scene: {nextSceneName}");
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
    
    private void PlaySound(string soundName)
    {
        if (string.IsNullOrEmpty(soundName)) return;
        if (AudioManager.instance != null)
            AudioManager.instance.Play(soundName);
    }
    
    private ZoneCamera FindZoneCamera()
    {
        ZoneCamera cam = ZoneCamera.Instance;
        if (cam == null) cam = FindObjectOfType<ZoneCamera>();
        return cam;
    }
}