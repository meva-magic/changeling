using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ZoneCamera : MonoBehaviour
{
    public static ZoneCamera Instance;
    
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;
    
    private bool isTransitioning;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (fadeImage != null)
        {
            fadeImage.color = Color.clear;
            fadeImage.gameObject.SetActive(false);
        }
    }
    
    public void MoveToZone(Vector3 newPosition, bool useFade = true)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionToZone(newPosition, useFade));
    }
    
    public void SnapToZone(Vector3 position)
    {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }
    
    private IEnumerator TransitionToZone(Vector3 newPosition, bool useFade)
    {
        isTransitioning = true;
        
        // Fade to black
        if (useFade && fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            float elapsed = 0f;
            Color color = fadeImage.color;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                fadeImage.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }
            fadeImage.color = new Color(color.r, color.g, color.b, 1f);
        }
        
        // INSTANTLY snap camera to new position
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
        
        // Small pause in black
        yield return new WaitForSeconds(0.1f);
        
        // Fade from black
        if (useFade && fadeImage != null)
        {
            float elapsed = 0f;
            Color color = fadeImage.color;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                fadeImage.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }
            
            fadeImage.color = new Color(color.r, color.g, color.b, 0f);
            fadeImage.gameObject.SetActive(false);
        }
        
        isTransitioning = false;
    }
    
    public Coroutine FadeToBlack(float duration = -1)
    {
        if (duration < 0) duration = fadeDuration;
        return StartCoroutine(FadeToBlackRoutine(duration));
    }
    
    private IEnumerator FadeToBlackRoutine(float duration)
    {
        if (fadeImage == null) yield break;
        
        fadeImage.gameObject.SetActive(true);
        float elapsed = 0f;
        Color color = fadeImage.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        
        fadeImage.color = new Color(color.r, color.g, color.b, 1f);
    }
    
    public Coroutine FadeFromBlack(float duration = -1)
    {
        if (duration < 0) duration = fadeDuration;
        return StartCoroutine(FadeFromBlackRoutine(duration));
    }
    
    private IEnumerator FadeFromBlackRoutine(float duration)
    {
        if (fadeImage == null) yield break;
        
        float elapsed = 0f;
        Color color = fadeImage.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        
        fadeImage.color = new Color(color.r, color.g, color.b, 0f);
        fadeImage.gameObject.SetActive(false);
    }
}