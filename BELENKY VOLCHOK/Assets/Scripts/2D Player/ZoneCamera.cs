using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ZoneCamera : MonoBehaviour
{
    public static ZoneCamera Instance;
    
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        if (fadeImage != null)
            fadeImage.gameObject.SetActive(true);
    }
    
    private void Start()
    {
        if (fadeImage != null)
        {
            fadeImage.color = Color.black;
            StartCoroutine(FadeFromBlack());
        }
    }
    
    public void SwitchZone(Zone newZone, Vector3 playerPos)
    {
        StartCoroutine(Transition(newZone, playerPos));
    }
    
    private IEnumerator Transition(Zone newZone, Vector3 playerPos)
    {
        yield return FadeToBlack();
        
        Camera.main.transform.position = new Vector3(
            newZone.transform.position.x,
            newZone.transform.position.y,
            Camera.main.transform.position.z
        );
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.transform.position = playerPos;
        
        yield return FadeFromBlack();
    }
    
    public IEnumerator FadeToBlack()
    {
        if (fadeImage == null) yield break;
        
        float elapsed = 0;
        Color color = fadeImage.color;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        color.a = 1;
        fadeImage.color = color;
    }
    
    public IEnumerator FadeFromBlack()
    {
        if (fadeImage == null) yield break;
        
        float elapsed = 0;
        Color color = fadeImage.color;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        color.a = 0;
        fadeImage.color = color;
    }
}